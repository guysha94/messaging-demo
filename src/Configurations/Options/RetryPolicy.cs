namespace MessagingDemo.Configurations.Options;

/// <summary>
/// Configuration for retry policy with exponential backoff.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>
    /// Initial retry interval. Default: 1 second.
    /// </summary>
    public TimeSpan InitialInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum retry interval. Default: 5 minutes.
    /// </summary>
    public TimeSpan MaxInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Backoff multiplier for exponential backoff. Default: 2.0.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum number of retries before giving up. Default: 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Whether to use jitter to prevent thundering herd. Default: true.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Calculate the delay for a specific retry attempt.
    /// </summary>
    /// <param name="attempt">The retry attempt number (0-based).</param>
    /// <returns>The delay to wait before retrying.</returns>
    public TimeSpan CalculateDelay(int attempt)
    {
        var delay = TimeSpan.FromMilliseconds(
            InitialInterval.TotalMilliseconds * Math.Pow(BackoffMultiplier, attempt));
        
        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, MaxInterval.TotalMilliseconds));

        if (UseJitter)
        {
            var random = new Random();
            var jitter = random.NextDouble() * 0.1 * delay.TotalMilliseconds; // 10% jitter
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds + jitter);
        }

        return delay;
    }
}

