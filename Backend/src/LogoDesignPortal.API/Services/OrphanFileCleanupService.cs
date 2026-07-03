using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.API.Services;

/// <summary>
/// Scans preview (Temporary) file storage and deletes orphan files (no DB row). Invoked on a schedule via Hangfire.
/// </summary>
public class OrphanFileCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileStorageService _fileStorage;
    private readonly string _localBasePath;
    private readonly ILogger<OrphanFileCleanupService> _logger;

    public OrphanFileCleanupService(
        IServiceProvider serviceProvider,
        IFileStorageService fileStorage,
        IConfiguration configuration,
        IOptions<StorageOptions> storageOptions,
        ILogger<OrphanFileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _fileStorage = fileStorage;
        var options = storageOptions.Value;
        _localBasePath = !string.IsNullOrWhiteSpace(options.Local.BasePath)
            ? options.Local.BasePath
            : configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
        _logger = logger;
    }

    /// <summary>Single cleanup pass; scheduled via Hangfire recurring job.</summary>
    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var previewKeys = (await _fileStorage.ListKeysAsync("Temporary", cancellationToken).ConfigureAwait(false)).ToList();
        if (previewKeys.Count == 0)
        {
            _logger.LogDebug("No files found in Temporary storage prefix.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var knownKeys = await GetKnownStorageKeysAsync(context, cancellationToken).ConfigureAwait(false);

        var deletedCount = 0;
        foreach (var key in previewKeys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var normalizedKey = StorageKeyHelper.ValidateAndNormalizeKey(key);
            if (knownKeys.Contains(normalizedKey))
            {
                continue;
            }

            try
            {
                await _fileStorage.DeleteAsync(normalizedKey, cancellationToken).ConfigureAwait(false);
                deletedCount++;
                _logger.LogInformation("Deleted orphan file: {Key}", normalizedKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete orphan file: {Key}", normalizedKey);
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Orphan file cleanup completed. Deleted {Count} orphan file(s) from preview storage.", deletedCount);
        }
    }

    private async Task<HashSet<string>> GetKnownStorageKeysAsync(
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
            .Select(p => StorageKeyHelper.ResolveKey(p!, _localBasePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
