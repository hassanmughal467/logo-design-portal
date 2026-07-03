using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>Verifies Hangfire job storage is reachable (Redis/Memory/SQL per configuration).</summary>
public sealed class HangfireStorageHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var storage = JobStorage.Current;
            if (storage == null)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Hangfire JobStorage is not initialized yet."));
            }

            var api = storage.GetMonitoringApi();
            _ = api.Servers();
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Hangfire storage is not reachable.", ex));
        }
    }
}
