namespace MessagingDemo.Models;

public record struct PublishContext
{
    public string Stream { get; set; }

    public IDictionary<string, string> Headers { get; set; }

    public string PartitionKey { get; set; }

    public PublishContext(string stream, IDictionary<string, string>? headers = null, string partitionKey = "")
    {
        Stream = stream;
        Headers = headers ?? new Dictionary<string, string>();
        PartitionKey = partitionKey;
    }
}