namespace MessagingDemo.Abstractions;

/// <summary>
/// Consumer interface for processing messages of a specific type.
/// </summary>
/// <typeparam name="TMessage">The message type to consume.</typeparam>
public interface IConsumer<TMessage> : IConsumer
    where TMessage : notnull
{
    /// <summary>
    /// Consumes a message from the stream.
    /// </summary>
    /// <param name="message">The consume context containing the message and metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task representing the asynchronous consume operation.</returns>
    ValueTask Consume(ConsumeContext<TMessage> message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for all consumers.
/// </summary>
public interface IConsumer
{
}