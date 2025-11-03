namespace MessagingDemo.Configurations.Options;

/// <summary>
/// Settings for a consumer endpoint.
/// </summary>
public sealed class EndpointSettings
{
    /// <summary>
    /// Number of messages to prefetch. If null, uses global default.
    /// </summary>
    public int? Prefetch { get; set; }

    /// <summary>
    /// Maximum concurrent message processing. If null, uses global default.
    /// </summary>
    public int? Concurrency { get; set; }

    /// <summary>
    /// Maximum number of delivery attempts before sending to DLQ. If null, uses global default.
    /// </summary>
    public int? MaxDeliveries { get; set; }

    /// <summary>
    /// Visibility timeout for message processing. Not native to Redis Streams, used for backoff calculations.
    /// </summary>
    public TimeSpan? VisibilityTimeout { get; set; }

    /// <summary>
    /// Retry policy for failed message processing. If null, no retries are performed.
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Whether to process messages in order. If true, concurrency is effectively 1.
    /// </summary>
    public bool OrderedProcessing { get; set; } = false;
}