namespace MessagingDemo.Abstractions;

/// <summary>
/// Message bus interface for publishing messages to Redis streams.
/// </summary>
public interface IBus
{
    /// <summary>
    /// Publishes a message to the specified Redis stream.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="stream">The stream name to publish to.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="headers">Optional headers to attach to the message.</param>
    /// <param name="partitionKey">Optional partition key for ordered processing (currently unused).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task Publish<T>(string stream, T message, IDictionary<string, string>? headers = null, string? partitionKey = null,
        CancellationToken ct = default);
}