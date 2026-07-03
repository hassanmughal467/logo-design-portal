namespace LogoDesignPortal.Application.Interfaces.Storage;

/// <summary>
/// Abstraction for blob/object storage. Local disk is the default; Cloudflare R2 plugs in for production.
/// Keys are relative paths (e.g. <c>Temporary/{guid}.png</c>) and must not contain directory traversal segments.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListKeysAsync(string prefix, CancellationToken cancellationToken = default);
}
