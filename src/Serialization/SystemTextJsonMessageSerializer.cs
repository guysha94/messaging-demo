using System.Text.Json.Serialization.Metadata;

namespace MessagingDemo.Serialization;

public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ContentType => "application/json";
    public byte[] Serialize<T>(T message) => JsonSerializer.SerializeToUtf8Bytes(message!, Options);
    public T Deserialize<T>(byte[] payload) => JsonSerializer.Deserialize<T>(payload, Options)!;
}