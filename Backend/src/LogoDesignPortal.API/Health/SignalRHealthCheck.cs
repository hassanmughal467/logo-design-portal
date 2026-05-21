using LogoDesignPortal.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>Verifies SignalR hub types are registered (backplane health is covered by Redis check when configured).</summary>
public sealed class SignalRHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _services;

    public SignalRHealthCheck(IServiceProvider services) => _services = services;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = _services.GetRequiredService<IHubContext<NotificationHub>>();
            return Task.FromResult(HealthCheckResult.Healthy("SignalR notification hub is registered."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("SignalR hub registration failed.", ex));
        }
    }
}
