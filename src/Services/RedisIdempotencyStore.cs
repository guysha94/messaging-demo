using MessagingDemo.Utils;

namespace MessagingDemo.Services;

/// <summary>
/// Redis-based idempotency store for duplicate message detection.
/// </summary>
public class RedisIdempotencyStore(IRedisConnectionPoolManager redisConnectionPoolManager) : IIdempotencyStore
{
    private IDatabaseAsync GetDatabase() => redisConnectionPoolManager.GetConnection().GetDatabase();

    /// <summary>
    /// Attempts to reserve a message ID for idempotency checking.
    /// </summary>
    /// <param name="messageId">The message ID to reserve.</param>
    /// <param name="ttl">Time to live for the reservation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the reservation was successful (message not seen before), false if duplicate.</returns>
    public Task<bool> TryReserveAsync(string messageId, TimeSpan ttl, CancellationToken ct)
        => GetDatabase().StringSetAsync(
            $"{Constants.IdempotencyKeyPrefix}{messageId}", 
            "1", 
            ttl, 
            When.NotExists);
}