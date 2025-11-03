namespace example.Models;

public record ExampleMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    
    public long TimeStamp { get; init; } = DateTimeOffset.UtcNow.Ticks;
}