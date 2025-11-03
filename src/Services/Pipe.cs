namespace MessagingDemo.Services;

internal sealed class Pipe<TContext>(IReadOnlyList<IMiddleware<TContext>> middlewares) : IPipe<TContext>
    where TContext : notnull
{
    private readonly struct Next(int i, Pipe<TContext> p) : IPipe<TContext>
    {
        public ValueTask Send(TContext ctx, CancellationToken ct) => p.InternalSend(ctx, i, ct);
    }
    private async ValueTask Invoke(TContext context, int i, CancellationToken ct = default)
    {
        if (i == middlewares.Count) return;
        await middlewares[i].Send(context, new Next(i + 1, this), ct);
    }

    public ValueTask Send(TContext context, CancellationToken ct = default) => Invoke(context, 0, ct);

    private ValueTask InternalSend(TContext context, int index, CancellationToken ct = default)
        => index >= middlewares.Count
            ? ValueTask.CompletedTask
            : middlewares[index].Send(context, new Next(index + 1, this), ct);
}