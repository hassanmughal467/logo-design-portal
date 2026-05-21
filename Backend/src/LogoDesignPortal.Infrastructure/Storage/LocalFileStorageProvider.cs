using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;

namespace LogoDesignPortal.Infrastructure.Storage;

/// <summary>
/// Local filesystem storage (default). Use shared NAS/SMB path or migrate to cloud provider for multi-instance.
/// </summary>
public sealed class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly string _root;

    public LocalFileStorageProvider(IConfiguration configuration)
    {
        _root = Path.GetFullPath(configuration["FileStorage:Path"] ?? "Files");
        Directory.CreateDirectory(_root);
    }

    public string RootPath => _root;

    public string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative path is required.", nameof(relativePath));

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (normalized.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Path traversal is not allowed.");

        var full = Path.GetFullPath(Path.Combine(_root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path resolves outside storage root.");

        return full;
    }

    public async Task WriteAsync(string relativePath, Stream content, CancellationToken cancellationToken = default)
    {
        var full = ResolvePath(relativePath);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolvePath(relativePath);
        if (!File.Exists(full))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolvePath(relativePath);
        return Task.FromResult(File.Exists(full));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolvePath(relativePath);
        if (File.Exists(full))
            File.Delete(full);
        return Task.CompletedTask;
    }
}
