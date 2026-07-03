using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Infrastructure.Storage;

/// <summary>
/// Local filesystem storage (default). Used in development and as the migration source for R2.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<StorageOptions> options, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _root = Path.GetFullPath(options.Value.Local.BasePath);
        Directory.CreateDirectory(_root);
    }

    public string RootPath => _root;

    public string ResolveFullPath(string key)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        var full = Path.GetFullPath(Path.Combine(_root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path resolves outside storage root.");
        }

        return full;
    }

    public async Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        var full = ResolveFullPath(normalized);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var fs = new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Stored local file {Key}", normalized);
        return normalized;
    }

    public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolveFullPath(key);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException($"Storage key not found: {key}");
        }

        Stream stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolveFullPath(key);
        return Task.FromResult(File.Exists(full));
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = ResolveFullPath(key);
        if (File.Exists(full))
        {
            File.Delete(full);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> ListKeysAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : StorageKeyHelper.ValidateAndNormalizeKey(prefix).TrimEnd('/');

        var searchRoot = string.IsNullOrEmpty(normalizedPrefix)
            ? _root
            : ResolveFullPath(normalizedPrefix);

        if (!Directory.Exists(searchRoot))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var keys = Directory
            .EnumerateFiles(searchRoot, "*", SearchOption.AllDirectories)
            .Select(path =>
            {
                var relative = Path.GetRelativePath(_root, path);
                return relative.Replace('\\', '/');
            })
            .Where(relative =>
                string.IsNullOrEmpty(normalizedPrefix)
                || relative.StartsWith(normalizedPrefix + "/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(relative, normalizedPrefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<IEnumerable<string>>(keys);
    }
}
