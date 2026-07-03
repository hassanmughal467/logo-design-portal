using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Infrastructure.Storage;

/// <summary>
/// Cloudflare R2 storage via the S3-compatible API.
/// </summary>
public sealed class R2FileStorageService : IFileStorageService, IDisposable
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;
    private readonly ILogger<R2FileStorageService> _logger;

    public R2FileStorageService(IOptions<StorageOptions> options, ILogger<R2FileStorageService> logger)
    {
        _logger = logger;
        var r2 = options.Value.R2;
        _bucketName = r2.BucketName;

        if (string.IsNullOrWhiteSpace(r2.AccountId))
        {
            throw new InvalidOperationException("Storage:R2:AccountId is required when Provider is R2.");
        }

        if (string.IsNullOrWhiteSpace(r2.AccessKeyId))
        {
            throw new InvalidOperationException("Storage:R2:AccessKeyId is required when Provider is R2.");
        }

        if (string.IsNullOrWhiteSpace(r2.SecretAccessKey))
        {
            throw new InvalidOperationException("Storage:R2:SecretAccessKey is required when Provider is R2.");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        var credentials = new BasicAWSCredentials(r2.AccessKeyId, r2.SecretAccessKey);
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = normalized,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            DisablePayloadSigning = true
        };

        await _client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Stored R2 object {Key}", normalized);
        return normalized;
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        try
        {
            var response = await _client.GetObjectAsync(_bucketName, normalized, cancellationToken).ConfigureAwait(false);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"Storage key not found: {key}", ex);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        await _client.DeleteObjectAsync(_bucketName, normalized, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalized = StorageKeyHelper.ValidateAndNormalizeKey(key);
        try
        {
            await _client.GetObjectMetadataAsync(_bucketName, normalized, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<IEnumerable<string>> ListKeysAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : StorageKeyHelper.ValidateAndNormalizeKey(prefix).TrimEnd('/');

        var keys = new List<string>();
        string? continuationToken = null;

        do
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = string.IsNullOrEmpty(normalizedPrefix) ? null : normalizedPrefix,
                ContinuationToken = continuationToken
            };

            var response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);
            keys.AddRange(response.S3Objects.Select(o => o.Key));
            continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
        }
        while (continuationToken != null);

        return keys;
    }

    public void Dispose() => _client.Dispose();
}
