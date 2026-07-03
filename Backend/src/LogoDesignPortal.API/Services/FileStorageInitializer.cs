using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Ensures local file storage directories exist at application startup when using the Local provider.
/// </summary>
public class FileStorageInitializer
{
    private readonly IConfiguration _configuration;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<FileStorageInitializer> _logger;

    public FileStorageInitializer(
        IConfiguration configuration,
        IOptions<StorageOptions> storageOptions,
        ILogger<FileStorageInitializer> logger)
    {
        _configuration = configuration;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates base, Temporary, and Permanent directories for local storage.
    /// Skipped when Provider is R2.
    /// </summary>
    public void Initialize()
    {
        if (_storageOptions.IsR2)
        {
            _logger.LogInformation("Storage provider is R2 — skipping local directory initialization.");
            return;
        }

        var basePath = !string.IsNullOrWhiteSpace(_storageOptions.Local.BasePath)
            ? _storageOptions.Local.BasePath
            : _configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");

        var temporaryPath = Path.Combine(basePath, "Temporary");
        var permanentPath = Path.Combine(basePath, "Permanent");
        var quotesPath = Path.Combine(basePath, "Quotes");

        try
        {
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(temporaryPath);
            Directory.CreateDirectory(permanentPath);
            Directory.CreateDirectory(quotesPath);
            _logger.LogInformation("File storage directories initialized at {Path}", basePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "Cannot create file storage directories at {Path}. File uploads will fail until write permissions are granted to the IIS App Pool identity (e.g. IIS AppPool\\HawkBE) for this path.",
                basePath);
        }
    }
}
