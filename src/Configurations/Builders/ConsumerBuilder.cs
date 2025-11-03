namespace MessagingDemo.Configurations.Builders;

public class ConsumerBuilder
{
    public string ConsumerGroup { get; set; } = string.Empty;

    public int MaxConcurrency { get; set; } = 1;

    public ConsumerOptions Build<T>(string streamName)
        where T : IConsumer
        => Build(typeof(T), streamName);

    public ConsumerOptions Build(Type type, string streamName)
        => new ConsumerOptions(type, streamName, ConsumerGroup, MaxConcurrency);
}