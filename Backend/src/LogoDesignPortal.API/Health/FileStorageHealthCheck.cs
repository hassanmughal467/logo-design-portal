using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Health;

public sealed class FileStorageHealthCheck : IHealthCheck
{
    private readonly IFileStorageService _fileStorage;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<FileStorageHealthCheck> _logger;

    public FileStorageHealthCheck(
        IFileStorageService fileStorage,
        IOptions<StorageOptions> storageOptions,
        ILogger<FileStorageHealthCheck> logger)
    {
        _fileStorage = fileStorage;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var probeKey = $".health/{Guid.NewGuid():N}";
            await using var payload = new MemoryStream("ok"u8.ToArray());
            await _fileStorage.UploadAsync(payload, probeKey, "text/plain", cancellationToken).ConfigureAwait(false);
            await _fileStorage.DeleteAsync(probeKey, cancellationToken).ConfigureAwait(false);

            var provider = _storageOptions.IsR2 ? "R2" : "Local";
            return HealthCheckResult.Healthy($"File storage ({provider}) is writable.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File storage health check failed.");
            return HealthCheckResult.Unhealthy("File storage is not writable.", ex);
        }
    }
}
