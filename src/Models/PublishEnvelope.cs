namespace MessagingDemo.Models;

public sealed class PublishEnvelope
{
    public required string Stream { get; init; }
    public required string TypeName { get; init; }
    public required byte[] Payload { get; init; }
    public IDictionary<string,string> Headers { get; } = new Dictionary<string, string>();
}