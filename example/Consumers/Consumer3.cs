namespace example.Consumers;

public class Consumer3(ILogger<Consumer3> logger) : IConsumer<ExampleMessage>
{
    public ValueTask Consume(ConsumeContext<ExampleMessage> ctx, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received message: {Message}", ctx);
        return ValueTask.CompletedTask;
    }
}