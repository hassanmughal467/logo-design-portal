using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

public sealed class FileStorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageHealthCheck> _logger;

    public FileStorageHealthCheck(IConfiguration configuration, ILogger<FileStorageHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var root = _configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        try
        {
            Directory.CreateDirectory(root);
            var probe = Path.Combine(root, $".health-{Guid.NewGuid():N}");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return Task.FromResult(HealthCheckResult.Healthy("File storage path is writable."));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File storage health check failed for {Path}", root);
            return Task.FromResult(HealthCheckResult.Unhealthy("File storage path is not writable.", ex));
        }
    }
}
