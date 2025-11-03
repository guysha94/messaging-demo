namespace MessagingDemo.Configurations.Options;

public record  StreamOptions
{

    public required Type Type { get; init; }
    public required string StreamName { get; init; }
    
    public string Prefix { get; set; }= string.Empty;
    
    public Dictionary<string, HashSet<ConsumerOptions>> ConsumersOptions { get; init; } = new();
}