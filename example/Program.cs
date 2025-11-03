using example.Services;
using MessagingDemo.Middlewares;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});
//
// builder.Services.AddMessaging(messagingBuilder =>
// {
//     messagingBuilder.AddStream<ExampleMessage>("stream-1", streamBuilder =>
//     {
//         streamBuilder.AddConsumer<Consumer1>(setup => { setup.ConsumerGroup = "group-a"; });


        // streamBuilder.AddConsumer<Consumer2>(setup =>
        // {
        //     setup.MaxConcurrency = 2;
        //     setup.ConsumerGroup = "group-a";
        // });
        //
        //
        //
        // streamBuilder.AddConsumer<Consumer3>(setup =>
        // {
        //     setup.ConsumerGroup = "group-b";
        //     setup.MaxConcurrency = 5;
        // });
//     });
// });
builder.Services.AddRedisStreamsMessaging(opts =>
{
    
    opts.AddConsumer<Consumer1, ExampleMessage>(
        stream: "stream-1",
        group: "group-a",
        endpoint: e =>
        {
            e.Prefetch = 1;
            e.Concurrency = 1;
            e.MaxDeliveries = 5;
        });
    opts.AddConsumer<Consumer2, ExampleMessage>(
        stream: "stream-1",
        group: "group-a",
        endpoint: e =>
        {
            e.Prefetch = 1;
            e.Concurrency = 1;
            e.MaxDeliveries = 5;
        });
    opts.AddConsumer<Consumer3, ExampleMessage>(
        stream: "stream-1",
        group: "group-b",
        endpoint: e =>
        {
            e.Prefetch = 1;
            e.Concurrency = 1;
            e.MaxDeliveries = 5;
        });
});

builder.Services.AddHostedService<ProducerA>();

var app = builder.Build();


app.Run();