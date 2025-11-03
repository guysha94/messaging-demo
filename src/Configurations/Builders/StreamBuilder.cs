namespace MessagingDemo.Configurations.Builders;

public class StreamBuilder
{
    private readonly LinkedList<(Type Type, ConsumerBuilder Builder)> _consumerOptions = new();


    public void AddConsumer<T>(Action<ConsumerBuilder>? setup = null)
        where T : IConsumer
    {
        var builder = new ConsumerBuilder();
        if (setup is not null)
        {
            setup(builder);
        }


        _consumerOptions.AddLast((typeof(T), builder));
    }

    public StreamOptions Build(Type type, string streamName, string prefix = "")
    {
        var consumers = _consumerOptions
            .GroupBy(x => x.Builder.ConsumerGroup)
            .ToDictionary(group => group.Key,
                group => group
                    .Select(builder => builder.Builder.Build(builder.Type, streamName))
                    .ToHashSet());

        return new StreamOptions
        {
            Type = type,
            StreamName = streamName,
            Prefix = prefix,
            ConsumersOptions = consumers
        };
    }
}