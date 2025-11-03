namespace MessagingDemo.Configurations;

internal sealed class ConsumerConfigurator<TConsumer,T>(string stream, string group, Action<EndpointSettings>? cfg)
    : IConsumerConfigurator
{
    public Type MessageType => typeof(T);
    
    public Type ConsumerType => typeof(TConsumer);
    public string Stream { get; } = stream;
    public string Group { get; } = group;
    public Action<EndpointSettings> Configure { get; } = cfg ?? (_ => { });
}