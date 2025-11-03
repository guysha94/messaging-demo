namespace MessagingDemo.Extensions;

public static class RegistrationExtensions
{
    /// <summary>
    /// Adds Redis Streams messaging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for Redis Streams options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisStreamsMessaging(
        this IServiceCollection services, Action<RedisStreamsOptions> configure)
    {
        var opts = new RedisStreamsOptions();
        configure(opts);

        // Validate configuration
        opts.Validate();

        // Set default concurrency if not specified
        if (opts.Concurrency == Constants.DefaultConcurrency)
        {
            opts.Concurrency = Environment.ProcessorCount;
        }

        AddRedisConnection(services, opts.ConnectionString, opts.RedisPoolSize, opts.MaxWorkers);

        services.AddSingleton(opts);

        services.AddSingleton(opts.Serializer);

        services.AddHostedService<RedisStreamWorker>();
        // publish side
        services.AddSingleton<IBus, RedisBus>();

        // discover & register consumers
        foreach (var t in opts.Consumers)
            services.AddScoped(t);
        
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        
        // pipeline filters (consume)
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware<ConsumeContext<object>>,
            LoggingMiddleware>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware<ConsumeContext<object>>,
            MetricsMiddleware>());
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IMiddleware<ConsumeContext<object>>, IdempotencyMiddleware>());

        // services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware<PublishEnvelope>,
        //     PublishMetricsFilter>());

        // worker per endpoint


        // health check
        services.AddHealthChecks().AddCheck<RedisStreamsHealthCheck>("redis_streams");

        return services;
    }

    /// <summary>
    /// Adds a consumer for a specific message type.
    /// </summary>
    /// <typeparam name="TConsumer">The consumer type.</typeparam>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="opts">The Redis Streams options.</param>
    /// <param name="stream">The stream name to consume from.</param>
    /// <param name="group">The consumer group name.</param>
    /// <param name="endpoint">Optional endpoint configuration.</param>
    /// <returns>The Redis Streams options for chaining.</returns>
    public static RedisStreamsOptions AddConsumer<TConsumer, TMessage>(
        this RedisStreamsOptions opts, string stream, string group,
        Action<EndpointSettings>? endpoint = null)
        where TConsumer : class, IConsumer<TMessage> where TMessage : notnull
    {
        opts.Consumers.Add(typeof(TConsumer));
        opts.Endpoints.Add(new ConsumerConfigurator<TConsumer, TMessage>(stream, group, endpoint));
        return opts;
    }

    public static void AddRedisConnection(this IServiceCollection services, string connectionString, int poolSize = 5,
        int maxWorkers = 10)
    {
        var configs = new RedisConfiguration
        {
            Name = "Default",
            AbortOnConnectFail = false,
            AllowAdmin = true,
            ConnectionString = connectionString,
            ConnectTimeout = 15000,
            Database = 0,
            PoolSize = poolSize,
            IsDefault = true,
            ConnectRetry = 5,
            ConnectionSelectionStrategy = ConnectionSelectionStrategy.RoundRobin,
            WorkCount = maxWorkers,
            SyncTimeout = 15000,
            SocketManagerOptions = SocketManager.SocketManagerOptions.UseHighPrioritySocketThreads,
            ConfigurationOptions =
            {
                TieBreaker = "",
                HeartbeatConsistencyChecks = true,
                HeartbeatInterval = TimeSpan.FromSeconds(5),
                KeepAlive = 360,
                ReconnectRetryPolicy = new ExponentialRetry(500, 10000)
            }
        };

        services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(configs);
        services.AddSingleton<IRedisConnectionPoolManager>(sp =>
            sp.GetRequiredService<IRedisClientFactory>().GetDefaultRedisClient().ConnectionPoolManager);
    }
}