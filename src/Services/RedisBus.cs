using MessagingDemo.Utils;

namespace MessagingDemo.Services;

/// <summary>
/// Redis-based message bus implementation for publishing messages to Redis streams.
/// </summary>
internal sealed class RedisBus(
    IRedisConnectionPoolManager redisConnectionPool,
    RedisStreamsOptions opts,
    IMessageSerializer serializer,
    IEnumerable<IMiddleware<PublishEnvelope>> middlewares)
    : IBus
{
    private readonly IPipe<PublishEnvelope> _pipe = new Pipe<PublishEnvelope>(middlewares.ToList());

    private IDatabaseAsync GetDatabase() => redisConnectionPool.GetConnection().GetDatabase();

    /// <summary>
    /// Publishes a message to the specified Redis stream.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="stream">The stream name.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="headers">Optional headers to attach to the message.</param>
    /// <param name="partitionKey">Optional partition key for ordered processing (currently unused).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task Publish<T>(string stream, T message, IDictionary<string, string>? headers = null, string? partitionKey = null, CancellationToken ct = default)
    {
        var payload = serializer.Serialize(message);
        var env = new PublishEnvelope
        {
            Stream = stream,
            TypeName = typeof(T).AssemblyQualifiedName!,
            Payload = payload,
        };
        
        if (headers is not null)
        {
            foreach (var kv in headers)
            {
                env.Headers[kv.Key] = kv.Value;
            }
        }

        // Run publish middleware pipeline
        await _pipe.Send(env, ct);

        var db = GetDatabase();
        
        // Pre-allocate array with exact size to avoid multiple allocations
        var headerCount = env.Headers.Count;
        var values = new NameValueEntry[3 + headerCount];
        values[0] = new(Constants.ContentTypeHeader, serializer.ContentType);
        values[1] = new(Constants.TypeHeader, env.TypeName);
        values[2] = new(Constants.PayloadHeader, payload);
        
        var index = 3;
        foreach (var kv in env.Headers)
        {
            values[index++] = new NameValueEntry(kv.Key, kv.Value);
        }

        _ = await db.StreamAddAsync(stream, values).ConfigureAwait(false);
    }
}