using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MessagingDemo.Runtime.Health;

public sealed class RedisStreamsHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _mux;
    public RedisStreamsHealthCheck(IConnectionMultiplexer mux) => _mux = mux;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        => _mux.IsConnected
            ? Task.FromResult(HealthCheckResult.Healthy())
            : Task.FromResult(HealthCheckResult.Unhealthy("Redis disconnected"));
}