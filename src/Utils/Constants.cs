namespace MessagingDemo.Utils;

internal static class Constants
{
    public const string IdempotencyKeyPrefix = "idempotency:";
    public const string DeliveryCountKeyFormat = "{0}:{1}:deliveries:{2}";
    public const string ScheduledMessagesKey = "scheduled:messages";
    public const string UndeliveredMessagesIndicator = ">";
    
    public static readonly TimeSpan DefaultIdempotencyTtl = TimeSpan.FromHours(1);
    public static readonly TimeSpan DefaultDeliveryCountTtl = TimeSpan.FromHours(6);
    public static readonly TimeSpan DefaultPendingRecoveryInterval = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(30);
    
    public const int DefaultMaxStreamLength = 10000;
    public const int DefaultPrefetchCount = 64;
    public const int DefaultConcurrency = 0; // 0 means use Environment.ProcessorCount
    public const int DefaultMaxDeliveries = 5;
    public const int DefaultRedisPoolSize = 5;
    public const int DefaultMaxWorkers = 10;
    
    public const string ContentTypeHeader = "content-type";
    public const string TypeHeader = "type";
    public const string PayloadHeader = "payload";
    public const string DlqReasonHeader = "dlq.reason";
    public const string DlqDetailHeader = "dlq.detail";
    public const string OriginalMessageIdHeader = "original.message.id";
    
    public const string DeserializeErrorReason = "deserialize_error";
    public const string MaxDeliveriesExceededReason = "max_deliveries_exceeded";
    public const string ConsumerErrorReason = "consumer_error";
}

