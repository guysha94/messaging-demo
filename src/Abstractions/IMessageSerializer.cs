namespace MessagingDemo.Abstractions;

/// <summary>
/// Interface for serializing and deserializing messages.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Gets the content type identifier for this serializer (e.g., "application/json").
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Serializes a message to a byte array.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as a byte array.</returns>
    byte[] Serialize<T>(T message);

    /// <summary>
    /// Deserializes a byte array to a message.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="payload">The serialized message.</param>
    /// <returns>The deserialized message.</returns>
    T Deserialize<T>(byte[] payload);
}