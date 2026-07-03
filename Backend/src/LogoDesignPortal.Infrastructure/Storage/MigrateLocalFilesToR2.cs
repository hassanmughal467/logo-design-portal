using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Logging;

namespace LogoDesignPortal.Infrastructure.Storage;

/// <summary>
/// One-time utility to copy existing local disk files into Cloudflare R2.
/// Run manually after configuring R2 credentials — not registered as a Hangfire job.
/// </summary>
public sealed class MigrateLocalFilesToR2
{
    public async Task MigrateAsync(
        string localBasePath,
        IFileStorageService r2Storage,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var baseFull = Path.GetFullPath(localBasePath);
        if (!Directory.Exists(baseFull))
        {
            logger.LogWarning("Local storage path does not exist: {Path}. Nothing to migrate.", baseFull);
            return;
        }

        var files = Directory.EnumerateFiles(baseFull, "*", SearchOption.AllDirectories).ToList();
        logger.LogInformation("Found {Count} local file(s) under {Path} to migrate.", files.Count, baseFull);

        var uploaded = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var filePath in files)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var relativeKey = Path.GetRelativePath(baseFull, filePath).Replace('\\', '/');

            try
            {
                if (await r2Storage.ExistsAsync(relativeKey, cancellationToken).ConfigureAwait(false))
                {
                    skipped++;
                    logger.LogInformation("Skipping {Key} — already exists in R2.", relativeKey);
                    continue;
                }

                var contentType = GuessContentType(filePath);
                await using var stream = File.OpenRead(filePath);
                await r2Storage.UploadAsync(stream, relativeKey, contentType, cancellationToken).ConfigureAwait(false);
                uploaded++;
                logger.LogInformation("Uploaded {Key} ({Index}/{Total}).", relativeKey, uploaded + skipped + failed, files.Count);
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex, "Failed to migrate {Key}.", relativeKey);
            }
        }

        logger.LogInformation(
            "Migration complete. Uploaded={Uploaded}, Skipped={Skipped}, Failed={Failed}, Total={Total}.",
            uploaded, skipped, failed, files.Count);
    }

    private static string GuessContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".ai" => "application/postscript",
            ".eps" => "application/postscript",
            ".psd" => "image/vnd.adobe.photoshop",
            _ => "application/octet-stream"
        };
    }
}
