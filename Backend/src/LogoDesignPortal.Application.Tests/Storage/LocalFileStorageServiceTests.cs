using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogoDesignPortal.Application.Configuration;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Storage;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task UploadAndDownload_RoundTripsContent()
    {
        var root = Path.Combine(Path.GetTempPath(), "ldp-storage-test-" + Guid.NewGuid());
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{StorageOptions.SectionName}:Provider"] = "Local",
                [$"{StorageOptions.SectionName}:Local:BasePath"] = root
            })
            .Build();

        var options = Microsoft.Extensions.Options.Options.Create(new StorageOptions { Local = new LocalStorageOptions { BasePath = root } });
        var service = new LocalFileStorageService(options, Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalFileStorageService>.Instance);

        await using (var ms = new MemoryStream("hello"u8.ToArray()))
        {
            await service.UploadAsync(ms, "Temporary/test.txt", "text/plain");
        }

        await using var read = await service.DownloadAsync("Temporary/test.txt");
        using var reader = new StreamReader(read);
        Assert.Equal("hello", await reader.ReadToEndAsync());
    }

    [Fact]
    public void ValidateAndNormalizeKey_RejectsTraversal()
    {
        Assert.Throws<InvalidOperationException>(() =>
            LogoDesignPortal.Application.Helpers.StorageKeyHelper.ValidateAndNormalizeKey("../outside.txt"));
    }
}
