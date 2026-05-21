using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogoDesignPortal.API.Health;

public sealed class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private const long MinimumFreeBytes = 512L * 1024 * 1024;

    public DiskSpaceHealthCheck(IConfiguration configuration) => _configuration = configuration;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var root = _configuration["FileStorage:Path"] ?? Directory.GetCurrentDirectory();
        var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(root)) ?? root);

        if (!drive.IsReady)
            return Task.FromResult(HealthCheckResult.Degraded("Storage drive is not ready."));

        if (drive.AvailableFreeSpace < MinimumFreeBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Low disk space on {drive.Name}: {drive.AvailableFreeSpace / (1024 * 1024)} MB free (minimum {MinimumFreeBytes / (1024 * 1024)} MB)."));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"Disk space OK ({drive.AvailableFreeSpace / (1024 * 1024)} MB free)."));
    }
}
