namespace LogoDesignPortal.Application.Interfaces.Storage;

/// <summary>
/// Abstraction for blob/object storage. Local disk is the default; cloud providers (S3, R2, Azure Blob) plug in here.
/// Paths are relative to the storage root and must not contain directory traversal segments.
/// </summary>
public interface IFileStorageProvider
{
    string RootPath { get; }

    string ResolvePath(string relativePath);

    Task WriteAsync(string relativePath, Stream content, CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
