namespace MessagingDemo.Configurations.Builders;

public class MessagingBuilder
{
    public string GlobalPrefix { get; set; } = "stream";
    
    public int RedisPoolSize { get; set; } = 5;
    
    public int MaxWorkers { get; set; } = 10;
    public string ConnectionString { get; set; } = "localhost:6379";

    public ResiliencePipeline ResiliencePipeline { get; set; } = ResiliencePipeline.Empty;

    internal readonly Dictionary<string, StreamOptions> StreamOptions = new();

    public void AddStream(Type type, string streamName, Action<StreamBuilder> setup)
    {
        var streamBuilder = new StreamBuilder();
        setup(streamBuilder);
        StreamOptions.Add(streamName, streamBuilder.Build(type, streamName, GlobalPrefix));
    }
    
    public void AddStream<T>(string streamName, Action<StreamBuilder> setup)
        => AddStream(typeof(T), streamName, setup);

    internal MessagingBuilder Build()
    {
        ResiliencePipeline ??= ResiliencePipeline.Empty;
        foreach (var s in StreamOptions.Values)
        {
            s.Prefix = GlobalPrefix;
        }

        return this;
    }
}