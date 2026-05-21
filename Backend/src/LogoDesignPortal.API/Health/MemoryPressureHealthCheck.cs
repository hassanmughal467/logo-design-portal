using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>
/// Degraded when process working set exceeds configured threshold (production memory pressure signal).
/// </summary>
public sealed class MemoryPressureHealthCheck : IHealthCheck
{
    private readonly long _degradedBytes;
    private readonly long _unhealthyBytes;

    public MemoryPressureHealthCheck(IConfiguration configuration)
    {
        var mb = configuration.GetSection("HealthChecks:Memory");
        _degradedBytes = mb.GetValue<long>("DegradedWorkingSetMb", 1024) * 1024 * 1024;
        _unhealthyBytes = mb.GetValue<long>("UnhealthyWorkingSetMb", 2048) * 1024 * 1024;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var ws = GC.GetTotalMemory(forceFullCollection: false);
        if (ws >= _unhealthyBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Working set {ws / (1024 * 1024)} MB exceeds unhealthy threshold."));
        }

        if (ws >= _degradedBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Working set {ws / (1024 * 1024)} MB exceeds degraded threshold."));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"Working set {ws / (1024 * 1024)} MB."));
    }
}
