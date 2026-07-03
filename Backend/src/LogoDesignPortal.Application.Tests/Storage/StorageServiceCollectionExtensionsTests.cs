using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Storage;
using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Storage;

public class StorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFileStorage_LocalProvider_RegistersLocalImplementation()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{StorageOptions.SectionName}:Provider"] = "Local",
                [$"{StorageOptions.SectionName}:Local:BasePath"] = Path.Combine(Path.GetTempPath(), "ldp-di-local-" + Guid.NewGuid())
            })
            .Build();

        services.AddLogging();
        services.AddFileStorage(config);
        var provider = services.BuildServiceProvider();

        Assert.IsType<LocalFileStorageService>(provider.GetRequiredService<IFileStorageService>());
    }

    [Fact]
    public void AddFileStorage_R2Provider_RegistersR2Implementation()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{StorageOptions.SectionName}:Provider"] = "R2",
                [$"{StorageOptions.SectionName}:R2:AccountId"] = "test-account",
                [$"{StorageOptions.SectionName}:R2:AccessKeyId"] = "test-key",
                [$"{StorageOptions.SectionName}:R2:SecretAccessKey"] = "test-secret",
                [$"{StorageOptions.SectionName}:R2:BucketName"] = "hawk-files"
            })
            .Build();

        services.AddLogging();
        services.AddFileStorage(config);
        var provider = services.BuildServiceProvider();

        Assert.IsType<R2FileStorageService>(provider.GetRequiredService<IFileStorageService>());
    }
}
