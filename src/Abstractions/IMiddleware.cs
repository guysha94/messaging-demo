namespace MessagingDemo.Abstractions;

/// <summary>
/// Middleware interface for processing messages in a pipeline.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IMiddleware<TContext> 
    where TContext : notnull
{
    /// <summary>
    /// Processes the context and optionally calls the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The context to process.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask Send(TContext context, IPipe<TContext> next, CancellationToken ct);
}