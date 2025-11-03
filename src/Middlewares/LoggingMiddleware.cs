using System.Diagnostics;

namespace MessagingDemo.Middlewares;

public sealed class LoggingMiddleware(ILogger<LoggingMiddleware> log) : IMiddleware<ConsumeContext<object>>
{
    public async ValueTask Send(ConsumeContext<object> ctx, IPipe<ConsumeContext<object>> next, CancellationToken ct)
    {
        var ts = Stopwatch.GetTimestamp();
        try
        {
            await next.Send(ctx, ct);
            log.LogInformation("Consumed {Type} {MessageId} from {Stream}/{Group} in {Elapsed}ms",
                ctx.Message.GetType().Name, ctx.MessageId, ctx.Stream, ctx.Group,
                Stopwatch.GetElapsedTime(ts).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error consuming {Type} {MessageId}", ctx.Message.GetType().Name, ctx.MessageId);
            throw;
        }
    }
}