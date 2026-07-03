using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Storage;

public class R2FileStorageServiceTests
{
    [Fact]
    public void Constructor_ThrowsWhenAccountIdMissing()
    {
        var options = Options.Create(new StorageOptions
        {
            R2 = new R2StorageOptions
            {
                AccountId = "",
                AccessKeyId = "key",
                SecretAccessKey = "secret",
                BucketName = "hawk-files"
            }
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new R2FileStorageService(options, NullLogger<R2FileStorageService>.Instance));

        Assert.Contains("AccountId", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ThrowsWhenAccessKeyIdMissing()
    {
        var options = Options.Create(new StorageOptions
        {
            R2 = new R2StorageOptions
            {
                AccountId = "account",
                AccessKeyId = "",
                SecretAccessKey = "secret",
                BucketName = "hawk-files"
            }
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new R2FileStorageService(options, NullLogger<R2FileStorageService>.Instance));

        Assert.Contains("AccessKeyId", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ThrowsWhenSecretAccessKeyMissing()
    {
        var options = Options.Create(new StorageOptions
        {
            R2 = new R2StorageOptions
            {
                AccountId = "account",
                AccessKeyId = "key",
                SecretAccessKey = "",
                BucketName = "hawk-files"
            }
        });

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new R2FileStorageService(options, NullLogger<R2FileStorageService>.Instance));

        Assert.Contains("SecretAccessKey", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_SucceedsWithValidCredentials()
    {
        var options = Options.Create(new StorageOptions
        {
            R2 = new R2StorageOptions
            {
                AccountId = "test-account",
                AccessKeyId = "test-key",
                SecretAccessKey = "test-secret",
                BucketName = "hawk-files"
            }
        });

        using var service = new R2FileStorageService(options, NullLogger<R2FileStorageService>.Instance);
        Assert.NotNull(service);
    }
}
