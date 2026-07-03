using LogoDesignPortal.Application.Interfaces.Storage;

namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Shared storage operations built on <see cref="IFileStorageService"/>.
/// </summary>
public static class FileStorageOperations
{
    public static async Task MoveAsync(
        IFileStorageService storage,
        string sourceKey,
        string destinationKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var source = StorageKeyHelper.ValidateAndNormalizeKey(sourceKey);
        var destination = StorageKeyHelper.ValidateAndNormalizeKey(destinationKey);

        if (!await storage.ExistsAsync(source, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        await using (var stream = await storage.DownloadAsync(source, cancellationToken).ConfigureAwait(false))
        {
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            buffer.Position = 0;
            await storage.UploadAsync(buffer, destination, contentType, cancellationToken).ConfigureAwait(false);
        }

        await storage.DeleteAsync(source, cancellationToken).ConfigureAwait(false);
    }
}
