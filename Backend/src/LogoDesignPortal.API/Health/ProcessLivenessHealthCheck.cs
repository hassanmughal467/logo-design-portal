using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>Kubernetes liveness: process is running and can respond (no external dependencies).</summary>
public sealed class ProcessLivenessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy("API process is alive."));
}
