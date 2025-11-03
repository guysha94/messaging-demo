namespace example.Consumers;

public class Consumer1(ILogger<Consumer1> logger) : IConsumer<ExampleMessage>
{
    public ValueTask Consume(ConsumeContext<ExampleMessage> ctx, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Received message: {Message}", ctx);
        return ValueTask.CompletedTask;
    }
}