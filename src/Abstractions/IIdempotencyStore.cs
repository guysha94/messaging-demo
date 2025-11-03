namespace MessagingDemo.Abstractions;

/// <summary>
/// Interface for idempotency checking to prevent duplicate message processing.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Attempts to reserve a message ID for idempotency checking.
    /// </summary>
    /// <param name="messageId">The message ID to reserve.</param>
    /// <param name="ttl">Time to live for the reservation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the reservation was successful (message not seen before), false if duplicate.</returns>
    Task<bool> TryReserveAsync(string messageId, TimeSpan ttl, CancellationToken ct);
}
