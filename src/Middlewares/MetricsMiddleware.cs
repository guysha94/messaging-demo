using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MessagingDemo.Middlewares;

/// <summary>
/// Middleware that tracks metrics for message consumption.
/// </summary>
public class MetricsMiddleware : IMiddleware<ConsumeContext<object>>
{
    private static readonly Meter Meter = new("MessagingDemo.RedisStreams.Messaging");
    private static readonly Counter<long> Consumed = Meter.CreateCounter<long>("redisstreams.consumed");
    private static readonly Counter<long> Failed = Meter.CreateCounter<long>("redisstreams.failed");
    private static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>(
        "redisstreams.processing.duration", "ms");

    public async ValueTask Send(ConsumeContext<object> ctx, IPipe<ConsumeContext<object>> next, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var tags = new TagList
        {
            { "message_type", ctx.Message.GetType().Name },
            { "stream", ctx.Stream },
            { "group", ctx.Group }
        };

        try
        {
            await next.Send(ctx, ct);
            Consumed.Add(1, tags);
        }
        catch
        {
            Failed.Add(1, tags);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            ProcessingDuration.Record(stopwatch.ElapsedMilliseconds, tags);
        }
    }
}