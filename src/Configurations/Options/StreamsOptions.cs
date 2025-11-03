using MessagingDemo.Serialization;
using MessagingDemo.Utils;

namespace MessagingDemo.Configurations.Options;

/// <summary>
/// Global options for Redis Streams messaging.
/// </summary>
public sealed class RedisStreamsOptions
{
    public string GlobalPrefix { get; set; } = "streams";

    public int RedisPoolSize { get; set; } = Constants.DefaultRedisPoolSize;

    public int MaxWorkers { get; set; } = Constants.DefaultMaxWorkers;

    public string ConnectionString { get; set; } = "localhost:6379";

    public string ConsumerAppName { get; set; } = Environment.MachineName;
    
    /// <summary>
    /// Number of messages to prefetch per read. Default: 64.
    /// </summary>
    public int PrefetchCount { get; set; } = Constants.DefaultPrefetchCount;
    
    /// <summary>
    /// Maximum concurrent message processing. Default: Environment.ProcessorCount.
    /// </summary>
    public int Concurrency { get; set; } = Constants.DefaultConcurrency; // 0 means use ProcessorCount
    
    /// <summary>
    /// Maximum stream length before trimming. Default: 10000.
    /// </summary>
    public int MaxStreamLength { get; set; } = Constants.DefaultMaxStreamLength;
    
    /// <summary>
    /// Block timeout for XREADGROUP. Default: 2 seconds.
    /// </summary>
    public TimeSpan BlockTimeout { get; set; } = TimeSpan.FromSeconds(2);
    
    /// <summary>
    /// Minimum idle time before claiming pending messages. Default: 1 minute.
    /// </summary>
    public TimeSpan MinIdleForClaim { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Maximum delivery attempts before sending to DLQ. Default: 5.
    /// </summary>
    public int MaxDeliveries { get; set; } = Constants.DefaultMaxDeliveries;
    
    /// <summary>
    /// Function to format stream names from message types.
    /// </summary>
    public Func<Type, string> StreamNameFormatter { get; set; } = t => t.Name.ToKebabCase();
    
    /// <summary>
    /// Function to format DLQ stream names from message types.
    /// </summary>
    public Func<Type, string> DlqStreamNameFormatter { get; set; } = t => $"{t.Name.ToKebabCase()}.dlq";
    
    /// <summary>
    /// Message serializer instance.
    /// </summary>
    public IMessageSerializer Serializer { get; set; } = new SystemTextJsonMessageSerializer();
    
    /// <summary>
    /// List of consumer types to register.
    /// </summary>
    public IList<Type> Consumers { get; } = [];
    
    /// <summary>
    /// List of consumer endpoint configurators.
    /// </summary>
    public IList<IConsumerConfigurator> Endpoints { get; } = [];
    
    /// <summary>
    /// Global middleware filters for consuming messages.
    /// </summary>
    public IList<IMiddleware<ConsumeContext<object>>> GlobalConsumeFilters { get; } = [];

    /// <summary>
    /// Global middleware filters for publishing messages.
    /// </summary>
    public IList<IMiddleware<PublishEnvelope>> GlobalPublishFilters { get; } = [];

    /// <summary>
    /// Idempotency store for duplicate message detection.
    /// </summary>
    public IIdempotencyStore? IdempotencyStore { get; set; }
    
    /// <summary>
    /// Whether to automatically create consumer groups if they don't exist. Default: true.
    /// </summary>
    public bool AutoCreateGroups { get; set; } = true;

    /// <summary>
    /// Resilience pipeline for handling transient failures.
    /// </summary>
    public ResiliencePipeline ResiliencePipeline { get; set; } = ResiliencePipeline.Empty;

    /// <summary>
    /// Validates the configuration and throws if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new ArgumentException("ConnectionString cannot be null or empty", nameof(ConnectionString));

        if (RedisPoolSize <= 0)
            throw new ArgumentException("RedisPoolSize must be greater than 0", nameof(RedisPoolSize));

        if (MaxWorkers <= 0)
            throw new ArgumentException("MaxWorkers must be greater than 0", nameof(MaxWorkers));

        if (PrefetchCount <= 0)
            throw new ArgumentException("PrefetchCount must be greater than 0", nameof(PrefetchCount));

        if (Concurrency < 0)
            throw new ArgumentException("Concurrency must be greater than or equal to 0", nameof(Concurrency));

        if (MaxStreamLength <= 0)
            throw new ArgumentException("MaxStreamLength must be greater than 0", nameof(MaxStreamLength));

        if (MaxDeliveries <= 0)
            throw new ArgumentException("MaxDeliveries must be greater than 0", nameof(MaxDeliveries));

        if (BlockTimeout <= TimeSpan.Zero)
            throw new ArgumentException("BlockTimeout must be greater than zero", nameof(BlockTimeout));

        if (MinIdleForClaim <= TimeSpan.Zero)
            throw new ArgumentException("MinIdleForClaim must be greater than zero", nameof(MinIdleForClaim));

        if (StreamNameFormatter == null)
            throw new ArgumentException("StreamNameFormatter cannot be null", nameof(StreamNameFormatter));

        if (DlqStreamNameFormatter == null)
            throw new ArgumentException("DlqStreamNameFormatter cannot be null", nameof(DlqStreamNameFormatter));

        if (Serializer == null)
            throw new ArgumentException("Serializer cannot be null", nameof(Serializer));
    }
}