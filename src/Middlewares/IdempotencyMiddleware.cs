namespace MessagingDemo.Middlewares;


public sealed class IdempotencyMiddleware : IMiddleware<ConsumeContext<object>>
{
    private readonly IIdempotencyStore _store;

    public IdempotencyMiddleware(IIdempotencyStore store)
    {
        _store = store;
    }
    public async ValueTask Send(ConsumeContext<object> ctx, IPipe<ConsumeContext<object>> next, CancellationToken ct)
    {
        if (!await _store.TryReserveAsync(ctx.MessageId, TimeSpan.FromHours(1), ct))
            return; // duplicate -> drop (already processed)

        await next.Send(ctx, ct);
    }
}