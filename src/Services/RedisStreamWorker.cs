using System.Collections.Concurrent;

namespace MessagingDemo.Services;

/// <summary>
/// Worker service that processes messages from Redis streams using consumer groups.
/// </summary>
public class RedisStreamWorker : IHostedLifecycleService
{
    private readonly ILogger<RedisStreamWorker> _logger;
    private readonly IRedisConnectionPoolManager _redisConnectionPool;
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisStreamsOptions _options;
    private readonly IPipe<ConsumeContext<object>> _pipe;
    private static readonly SemaphoreSlim _mutex = new(1, 1);

    private IDatabase GetDatabase() => _redisConnectionPool.GetConnection().GetDatabase();
    private IList<IConsumerConfigurator> _endpoints => _options.Endpoints;
    private string _consumerName => _options.ConsumerAppName;
    private TimeSpan _blockTimeout => _options.BlockTimeout;

    private static readonly string TrimScript = LuaScript.Prepare(Constants.TrimScript).ExecutableScript;


    private static readonly
        ConcurrentDictionary<(Type consumerType, Type messageType), (ConstructorInfo ctxCtor, MethodInfo consume)>
        _dispatchCache
            = new();

    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();


    public RedisStreamWorker(
        ILogger<RedisStreamWorker> logger,
        IRedisConnectionPoolManager redisConnectionPool,
        IServiceProvider serviceProvider,
        RedisStreamsOptions options,
        IEnumerable<IMiddleware<ConsumeContext<object>>> middlewares
    )
    {
        _logger = logger;
        _redisConnectionPool = redisConnectionPool;
        _serviceProvider = serviceProvider;
        _options = options;
        _pipe = new Pipe<ConsumeContext<object>>(middlewares.ToList());
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RedisStreamWorker");
        try
        {
            var db = GetDatabase();
            var latency = await db.PingAsync(flags: CommandFlags.PreferMaster);

            _logger.LogInformation(
                "Connected to Redis. Ping latency: {Latency} ms", latency.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis.");
            throw;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start RedisStreamWorker");
        if (_options.AutoCreateGroups)
        {
            await Task.WhenAll(_endpoints
                    .Select(e => CreateConsumerGroupsAsync(e.Stream, e.Group)))
                .ConfigureAwait(false);
        }
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started RedisStreamWorker");

        var tasks = _endpoints.Select(ep => RunEndpointLoop(ep, cancellationToken)).ToArray();
        return Task.WhenAll(tasks);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RedisStreamWorker");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stop RedisStreamWorker");
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopped RedisStreamWorker");
        return Task.CompletedTask;
    }

    private async Task RunEndpointLoop(IConsumerConfigurator ep, CancellationToken ct)
    {
        var db = GetDatabase();
        var settings = new EndpointSettings();
        ep.Configure(settings);

        var prefetch = settings.Prefetch ?? _options.PrefetchCount;
        var concurrency = settings.Concurrency ?? _options.Concurrency;
        var maxDeliveries = settings.MaxDeliveries ?? _options.MaxDeliveries;

        _ = Task.Run(() => PendingRecoveryLoop(ep, ct), ct);

        // Cache consumer name - only needs to be unique per endpoint loop
        var consumerName = $"{_consumerName}-{Environment.ProcessId}-{Guid.NewGuid():N}";

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await db.StreamTrimAsync(
                    ep.Stream, _options.MaxStreamLength, true,
                    CommandFlags.FireAndForget
                );
                var entries = await db.StreamReadGroupAsync(
                    key: ep.Stream,
                    groupName: ep.Group,
                    consumerName: consumerName,
                    count: prefetch,
                    noAck: false, // We handle ACK manually after processing
                    flags: CommandFlags.PreferReplica);

                if (entries.Length == 0) continue;

                // Process entries - OrderBy removed as Redis Streams already guarantees order
                var tasks = new List<Task>(Math.Min(concurrency, entries.Length));
                foreach (var entry in entries)
                {
                    // Wait for concurrency slot if needed
                    if (tasks.Count >= concurrency)
                    {
                        var completed = await Task.WhenAny(tasks);
                        tasks.Remove(completed);
                    }

                    tasks.Add(ProcessEntry(ep, entry, db, settings, maxDeliveries, ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                await db.ScriptEvaluateAsync(TrimScript, [ep.Stream], [1000, "~"]).ConfigureAwait(false);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in stream loop for {Stream}/{Group}", ep.Stream, ep.Group);
            }
        }
    }

    private async Task ProcessEntry(IConsumerConfigurator ep, StreamEntry e, IDatabase db,
        EndpointSettings settings, int maxDeliveries, CancellationToken ct)
    {
        // Optimize: Build dictionary once and reuse
        RedisValue? payload = null;
        RedisValue? typeNameValue = null;
        var headers = new Dictionary<string, string>(e.Values.Length);

        foreach (var value in e.Values)
        {
            var name = value.Name.ToString();
            if (name == Constants.PayloadHeader)
            {
                payload = value.Value;
            }
            else if (name == Constants.TypeHeader)
            {
                typeNameValue = value.Value;
            }
            else if (name != Constants.ContentTypeHeader)
            {
                headers[name] = value.Value.ToString();
            }
        }

        // Validate required fields
        if (!payload.HasValue || !typeNameValue.HasValue)
        {
            _logger.LogWarning(
                "Message {MessageId} missing required fields (payload or type), acknowledging and skipping",
                e.Id);
            await db.StreamAcknowledgeAsync(ep.Stream, ep.Group, e.Id);
            return;
        }

        // Cache type lookups
        var typeName = typeNameValue.Value.ToString();
        Type messageType;
        try
        {
            messageType = _typeCache.GetOrAdd(typeName, t =>
            {
                var type = Type.GetType(t);
                if (type == null)
                {
                    throw new TypeLoadException($"Type '{t}' not found");
                }

                return type;
            });
        }
        catch (TypeLoadException)
        {
            _logger.LogError("Cannot resolve message type {TypeName} for message {MessageId}, sending to DLQ",
                typeName, e.Id);
            await SendToDlq(ep, db, e.Values, e.Id, Constants.DeserializeErrorReason,
                $"Type '{typeName}' not found");
            await db.StreamAcknowledgeAsync(ep.Stream, ep.Group, e.Id);
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var serializer = services.GetRequiredService<IMessageSerializer>();

        object message;
        try
        {
            message = Deserialize(serializer, messageType, (byte[])payload.Value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message {MessageId} of type {TypeName}, sending to DLQ",
                e.Id, typeName);
            await SendToDlq(ep, db, e.Values, e.Id, Constants.DeserializeErrorReason, ex.Message);
            await db.StreamAcknowledgeAsync(ep.Stream, ep.Group, e.Id);
            return;
        }

        var deliveries = await IncrementDeliveryCount(ep, db, e.Id);

        var ctxObj = new ConsumeContext<object>(
            message, ep.Stream, ep.Group, e.Id.ToString(), headers, services);

        try
        {
            // 1) Run global filters (logging/metrics/etc.)
            await _pipe.Send(ctxObj, ct);

            // 2) Dispatch to the typed consumer
            await InvokeConsumer(ep, services, messageType, message, e.Id.ToString(), headers, ct);

            // 3) ACK and cleanup on success
            await db.StreamAcknowledgeAsync(ep.Stream, ep.Group, e.Id);
            await ResetDeliveryCount(ep, db, e.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Message {MessageId} processing was canceled", e.Id);
            // Don't ACK - let it be redelivered
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} (delivery attempt {Attempt}/{Max})",
                e.Id, deliveries, maxDeliveries);

            if (deliveries >= maxDeliveries)
            {
                _logger.LogWarning("Message {MessageId} exceeded max deliveries ({Max}), sending to DLQ",
                    e.Id, maxDeliveries);
                await SendToDlq(ep, db, e.Values, e.Id, Constants.MaxDeliveriesExceededReason, ex.Message);
                await db.StreamAcknowledgeAsync(ep.Stream, ep.Group, e.Id);
                await ResetDeliveryCount(ep, db, e.Id);
            }
            // Otherwise, don't ACK - let Redis Streams redeliver based on pending mechanism
        }
    }

    private async Task InvokeConsumer(
        IConsumerConfigurator ep,
        IServiceProvider services,
        Type messageType,
        object message,
        string messageId,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct)
    {
        var consumer = services.GetRequiredService(ep.ConsumerType);

        var key = (ep.ConsumerType, messageType);
        if (!_dispatchCache.TryGetValue(key, out var tuple))
        {
            var ctxType = typeof(ConsumeContext<>).MakeGenericType(messageType);
            var ctxCtor = ctxType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single(c => c.GetParameters().Length == 6);
            var consume = ep.ConsumerType.GetMethod("Consume", new[] { ctxType, typeof(CancellationToken) })
                          ?? throw new MissingMethodException(
                              $"{ep.ConsumerType.Name}.Consume({ctxType.Name}, CancellationToken) not found");
            tuple = (ctxCtor, consume);
            _dispatchCache[key] = tuple;
        }


        var ctx = tuple.ctxCtor.Invoke([message, ep.Stream, ep.Group, messageId, headers, services]);

        var task = (ValueTask)tuple.consume.Invoke(consumer, [ctx, ct])!;
        await task.ConfigureAwait(false);
    }

    private async Task SendToDlq(IConsumerConfigurator ep, IDatabaseAsync db,
        NameValueEntry[] values, RedisValue id, string reason, string? detail)
    {
        try
        {
            var dlq = _options.DlqStreamNameFormatter(ep.MessageType);
            // Pre-allocate with exact size
            var fields = new NameValueEntry[values.Length + 3];
            Array.Copy(values, fields, values.Length);
            fields[values.Length] = new(Constants.DlqReasonHeader, reason);
            fields[values.Length + 1] = new(Constants.DlqDetailHeader, detail ?? string.Empty);
            fields[values.Length + 2] = new(Constants.OriginalMessageIdHeader, id);

            await db.StreamAddAsync(dlq, fields);
            _logger.LogInformation("Message {MessageId} sent to DLQ {DlqStream} (reason: {Reason})",
                id, dlq, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message {MessageId} to DLQ (reason: {Reason})",
                id, reason);
            // Re-throw to ensure the error is not silently swallowed
            throw;
        }
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> _deserializeMethodCache = new();

    private static object Deserialize(IMessageSerializer serializer, Type t, byte[] payload)
    {
        var method = _deserializeMethodCache.GetOrAdd(t, type =>
        {
            var deserializeMethod = typeof(IMessageSerializer).GetMethod(nameof(IMessageSerializer.Deserialize));
            return deserializeMethod!.MakeGenericMethod(type);
        });

        return method.Invoke(serializer, [payload])!;
    }

    private async Task PendingRecoveryLoop(IConsumerConfigurator ep, CancellationToken ct)
    {
        var db = GetDatabase();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var info = await db.StreamPendingAsync(ep.Stream, ep.Group);
                    if (info.PendingMessageCount > 0)
                    {
                        // fetch a page of idle messages and claim any idle past threshold
                        var pendings = await db
                            .StreamPendingMessagesAsync(ep.Stream, ep.Group, 100, _consumerName)
                            .ConfigureAwait(false);
                        foreach (var p in pendings)
                        {
                            try
                            {
                                await db.StreamClaimAsync(
                                        key: ep.Stream,
                                        consumerGroup: ep.Group,
                                        claimingConsumer: _consumerName,
                                        minIdleTimeInMs: (long)_options.MinIdleForClaim.TotalMilliseconds,
                                        messageIds: [p.MessageId])
                                    .ConfigureAwait(false);
                            }
                            catch
                            {
                                /* ignore */
                            }
                        }
                    }
                }
                catch
                {
                    /* ignore transient */
                }

                await Task.Delay(Constants.DefaultPendingRecoveryInterval, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pending recovery loop for {Stream}/{Group}", ep.Stream, ep.Group);
        }
    }

    private async Task<long> IncrementDeliveryCount(IConsumerConfigurator ep, IDatabaseAsync db, RedisValue id)
    {
        var key = GetDeliveryCountKey(ep.Stream, ep.Group, id);
        var count = await db.StringIncrementAsync(key);
        if (count == 1) await db.KeyExpireAsync(key, Constants.DefaultDeliveryCountTtl);
        return count;
    }


    private Task ResetDeliveryCount(IConsumerConfigurator ep, IDatabaseAsync db, RedisValue id)
        => db.KeyDeleteAsync(GetDeliveryCountKey(ep.Stream, ep.Group, id));


    private RedisKey GetDeliveryCountKey(string stream, string group, RedisValue id)
        => string.Format(Constants.DeliveryCountKeyFormat, stream, group, id);

    private async Task CreateConsumerGroupsAsync(string streamName, string groupName)
    {
        await _mutex.WaitAsync();
        try
        {
            var db = GetDatabase();
            if (!await db.KeyExistsAsync(streamName) ||
                (await db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0");
            }
        }
        finally
        {
            _mutex.Release();
        }
    }
}