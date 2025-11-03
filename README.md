# MessagingDemo

A MassTransit-like messaging library built on top of Redis Streams for .NET.

## Features

- 🚀 **High Performance**: Optimized for throughput with minimal allocations
- 🔄 **Consumer Groups**: Support for Redis Streams consumer groups
- 🛡️ **Reliability**: Built-in retry policies, dead letter queues, and idempotency
- 📊 **Observability**: Metrics and logging middleware out of the box
- 🔌 **Extensible**: Middleware pipeline for custom processing
- 💪 **Production Ready**: Health checks, graceful shutdown, and error handling

## Installation

Add the package from GitHub Packages:

```xml
<PackageReference Include="MessagingDemo" Version="1.0.0" />
```

You'll need to configure GitHub Packages authentication. See [Using GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#authenticating-to-github-packages) for details.

## Quick Start

### Configuration

```csharp
builder.Services.AddRedisStreamsMessaging(opts =>
{
    opts.ConnectionString = "localhost:6379";
    opts.AddConsumer<MyConsumer, MyMessage>(
        stream: "my-stream",
        group: "my-group",
        endpoint: e =>
        {
            e.Prefetch = 64;
            e.Concurrency = 8;
            e.MaxDeliveries = 5;
        });
});
```

### Publishing Messages

```csharp
public class MyService
{
    private readonly IBus _bus;
    
    public MyService(IBus bus) => _bus = bus;
    
    public async Task PublishMessage()
    {
        await _bus.Publish("my-stream", new MyMessage 
        { 
            Content = "Hello World" 
        });
    }
}
```

### Consuming Messages

```csharp
public class MyConsumer : IConsumer<MyMessage>
{
    public ValueTask Consume(ConsumeContext<MyMessage> ctx, CancellationToken ct)
    {
        Console.WriteLine($"Received: {ctx.Message.Content}");
        return ValueTask.CompletedTask;
    }
}
```

## Configuration Options

### Global Options

- `ConnectionString`: Redis connection string
- `PrefetchCount`: Number of messages to prefetch (default: 64)
- `Concurrency`: Maximum concurrent message processing (default: ProcessorCount)
- `MaxDeliveries`: Maximum delivery attempts before DLQ (default: 5)
- `MaxStreamLength`: Maximum stream length before trimming (default: 10000)

### Endpoint Options

- `Prefetch`: Override global prefetch count
- `Concurrency`: Override global concurrency
- `MaxDeliveries`: Override global max deliveries
- `RetryPolicy`: Configure retry with exponential backoff
- `OrderedProcessing`: Process messages in order (default: false)

## Features

### Idempotency

Messages are automatically checked for duplicates using Redis-based idempotency store.

### Dead Letter Queue

Messages that exceed max delivery attempts are automatically sent to a DLQ stream.

### Middleware Pipeline

Add custom middleware for logging, metrics, or processing:

```csharp
public class CustomMiddleware : IMiddleware<ConsumeContext<object>>
{
    public async ValueTask Send(ConsumeContext<object> ctx, IPipe<ConsumeContext<object>> next, CancellationToken ct)
    {
        // Pre-processing
        await next.Send(ctx, ct);
        // Post-processing
    }
}
```

### Health Checks

Built-in health checks are automatically registered:

```csharp
app.MapHealthChecks("/health");
```

## Performance

The library is optimized for high-throughput scenarios:

- Efficient dictionary lookups
- Cached type and method resolution
- Pre-allocated arrays
- Minimal LINQ allocations in hot paths

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

