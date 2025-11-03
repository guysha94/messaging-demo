namespace MessagingDemo.Models;

/// <summary>
/// Context containing message and metadata for consumption.
/// </summary>
/// <typeparam name="T">The message type.</typeparam>
public readonly record struct ConsumeContext<T>
    where T : notnull
{
    /// <summary>
    /// The message being consumed.
    /// </summary>
    public T Message { get; }

    /// <summary>
    /// The Redis stream name.
    /// </summary>
    public string Stream { get; }

    /// <summary>
    /// The consumer group name.
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// The Redis stream message ID.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// Message headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes a new instance of the ConsumeContext.
    /// </summary>
    public ConsumeContext(
        T message, string stream, string group, string messageId,
        IReadOnlyDictionary<string, string> headers, IServiceProvider services)
    {
        Message = message;
        Stream = stream;
        Group = group;
        MessageId = messageId;
        Headers = headers;
        Services = services;
    }

    /// <summary>
    /// Gets a required service from the service provider.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    public TService GetRequiredService<TService>() where TService : notnull
        => Services.GetRequiredService<TService>();

    /// <summary>
    /// Deconstructs the context into its components.
    /// </summary>
    public void Deconstruct(out T message, out string stream, out string group, out string messageId,
        out IReadOnlyDictionary<string, string> headers, out IServiceProvider services)
    {
        message = Message;
        stream = Stream;
        group = Group;
        messageId = MessageId;
        headers = Headers;
        services = Services;
    }
}