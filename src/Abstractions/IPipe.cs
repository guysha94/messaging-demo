namespace MessagingDemo.Abstractions;

/// <summary>
/// Pipeline interface for processing contexts through middleware.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IPipe<in TContext>
    where TContext : notnull
{
    /// <summary>
    /// Sends the context through the pipeline.
    /// </summary>
    /// <param name="context">The context to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Send(TContext context, CancellationToken ct);
}