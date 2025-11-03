namespace MessagingDemo.Abstractions;

public interface IConsumerConfigurator
{
    Type MessageType { get; }
    
    Type ConsumerType { get; }
    string Stream { get; }
    string Group { get; }
    Action<EndpointSettings> Configure { get; }
}