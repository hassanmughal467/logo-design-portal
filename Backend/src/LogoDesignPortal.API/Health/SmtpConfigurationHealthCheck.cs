using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

/// <summary>
/// Reports degraded when SMTP is not configured (email features may be limited).
/// </summary>
public sealed class SmtpConfigurationHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SmtpConfigurationHealthCheck(IConfiguration configuration) => _configuration = configuration;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var server = _configuration["Email:SmtpServer"];
        var user = _configuration["Email:SmtpUsername"];
        var password = _configuration["Email:SmtpPassword"];
        var from = _configuration["Email:FromEmail"];

        if (string.IsNullOrWhiteSpace(server))
        {
            return Task.FromResult(HealthCheckResult.Degraded("Email:SmtpServer is not configured."));
        }

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
        {
            return Task.FromResult(HealthCheckResult.Degraded("SMTP credentials or FromEmail are not configured; password reset emails may be disabled."));
        }

        return Task.FromResult(HealthCheckResult.Healthy("SMTP is configured."));
    }
}
