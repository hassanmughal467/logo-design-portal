using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Background service that periodically scans the preview (Temporary) file storage
/// and deletes orphan files: files on disk that have no corresponding database record.
/// Runs every 24 hours. Never touches Reference or Final (Permanent) storage.
/// </summary>
public class OrphanFileCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrphanFileCleanupService> _logger;
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(24);

    public OrphanFileCleanupService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OrphanFileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrphanFileCleanupService started. Will run every 24 hours.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Orphan file cleanup failed. Will retry at next interval.");
            }

            var intervalMinutes = _configuration.GetValue<int?>("FileStorage:OrphanCleanupIntervalMinutes");
            var interval = intervalMinutes.HasValue && intervalMinutes.Value > 0
                ? TimeSpan.FromMinutes(intervalMinutes.Value)
                : DefaultInterval;

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("OrphanFileCleanupService stopped.");
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        var fileStoragePath = _configuration["FileStorage:Path"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        var previewStoragePath = Path.Combine(fileStoragePath, "Temporary");

        if (!Directory.Exists(previewStoragePath))
        {
            _logger.LogDebug("Preview storage directory does not exist: {Path}", previewStoragePath);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Load all file paths from LogoFiles and RevisionFiles
        var knownPaths = await GetKnownFilePathsAsync(context, cancellationToken);

        var deletedCount = 0;
        var files = Directory.EnumerateFiles(previewStoragePath, "*", SearchOption.TopDirectoryOnly);

        foreach (var filePath in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var normalizedPath = NormalizePath(filePath);

            if (!knownPaths.Contains(normalizedPath))
            {
                try
                {
                    File.Delete(filePath);
                    deletedCount++;
                    _logger.LogInformation("Deleted orphan file: {Path}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete orphan file: {Path}", filePath);
                }
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Orphan file cleanup completed. Deleted {Count} orphan file(s) from preview storage.", deletedCount);
        }
    }

    private static async Task<HashSet<string>> GetKnownFilePathsAsync(
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        // LogoFiles: Preview and Revision types are stored in Temporary folder
        var logoPaths = await context.LogoFiles
            .AsNoTracking()
            .Where(f => f.FilePath != null && f.FilePath.Length > 0)
            .Select(f => f.FilePath)
            .ToListAsync(cancellationToken);

        // RevisionFiles: also stored in Temporary folder
        var revisionPaths = await context.RevisionFiles
            .AsNoTracking()
            .Where(f => f.FilePath != null && f.FilePath.Length > 0)
            .Select(f => f.FilePath)
            .ToListAsync(cancellationToken);

        var allPaths = logoPaths.Concat(revisionPaths)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(NormalizePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return allPaths;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        try
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return path.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
