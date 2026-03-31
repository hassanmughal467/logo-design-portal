using LogoDesignPortal.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Scans preview (Temporary) file storage and deletes orphan files (no DB row). Invoked on a schedule via Hangfire (not IHostedService) so only cluster workers run it.
/// </summary>
public class OrphanFileCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrphanFileCleanupService> _logger;

    public OrphanFileCleanupService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OrphanFileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Single cleanup pass; scheduled via Hangfire recurring job (not duplicated per instance when Redis storage is used).</summary>
    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
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

        var knownPaths = await GetKnownFilePathsAsync(context, cancellationToken).ConfigureAwait(false);

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
        var logoPaths = await context.LogoFiles
            .AsNoTracking()
            .Where(f => f.FilePath != null && f.FilePath.Length > 0)
            .Select(f => f.FilePath)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var revisionPaths = await context.RevisionFiles
            .AsNoTracking()
            .Where(f => f.FilePath != null && f.FilePath.Length > 0)
            .Select(f => f.FilePath)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return logoPaths.Concat(revisionPaths)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(NormalizePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
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
