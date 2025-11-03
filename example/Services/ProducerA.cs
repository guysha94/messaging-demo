namespace example.Services;

public class ProducerA(IBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string stream = "stream-1";

        for (var i = 1; i < 5; ++i)
        {
            var messages = Enumerable.Range(1, 5_001)
                .Select(j => new ExampleMessage { Content = $"Message {i * j} from Producer A" });


            await Task.WhenAll(
                messages.Select(m => bus.Publish(stream, m, headers: new Dictionary<string, string>
                {
                    { "producer", "A" },
                    { "message-id", m.Id }
                }, ct: stoppingToken))
            );
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}