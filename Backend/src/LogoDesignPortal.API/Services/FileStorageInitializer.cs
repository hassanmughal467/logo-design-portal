using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Ensures file storage directories exist at application startup.
/// Prevents lazy creation on first upload and surfaces permission issues early.
/// </summary>
public class FileStorageInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageInitializer> _logger;

    public FileStorageInitializer(IConfiguration configuration, ILogger<FileStorageInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Creates Files, Files/Temporary, and Files/Permanent directories if they do not exist.
    /// Does not delete or overwrite existing files. Logs a warning if write permissions are missing.
    /// </summary>
    public void Initialize()
    {
        var basePath = _configuration["FileStorage:Path"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");

        var temporaryPath = Path.Combine(basePath, "Temporary");
        var permanentPath = Path.Combine(basePath, "Permanent");

        try
        {
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(temporaryPath);
            Directory.CreateDirectory(permanentPath);
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
