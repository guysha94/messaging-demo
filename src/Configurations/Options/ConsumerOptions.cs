namespace MessagingDemo.Configurations.Options;

public readonly record struct ConsumerOptions
{
    
    public Guid Id { get; init; } = Guid.NewGuid();
    public Type Type { get; init; }

    public string StreamName { get; init; }
    public string ConsumerGroup { get; init; }

    public int MaxConcurrency { get; init; }

    public ConsumerOptions(Type type, string streamName, string consumerGroup, int maxConcurrency)
    {
        Type = type;
        StreamName = streamName;
        ConsumerGroup = consumerGroup;
        MaxConcurrency = maxConcurrency;
    }
}