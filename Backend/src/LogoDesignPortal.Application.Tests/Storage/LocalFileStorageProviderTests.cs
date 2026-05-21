using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Storage;

public class LocalFileStorageProviderTests
{
    [Fact]
    public async Task WriteAndRead_RoundTripsContent()
    {
        var root = Path.Combine(Path.GetTempPath(), "ldp-storage-test-" + Guid.NewGuid());
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Path"] = root })
            .Build();

        var provider = new LocalFileStorageProvider(config);
        await using (var ms = new MemoryStream("hello"u8.ToArray()))
        {
            await provider.WriteAsync("Temporary/test.txt", ms);
        }

        await using var read = await provider.OpenReadAsync("Temporary/test.txt");
        Assert.NotNull(read);
        using var reader = new StreamReader(read);
        Assert.Equal("hello", await reader.ReadToEndAsync());
    }

    [Fact]
    public void ResolvePath_RejectsTraversal()
    {
        var root = Path.Combine(Path.GetTempPath(), "ldp-storage-traverse-" + Guid.NewGuid());
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Path"] = root })
            .Build();

        var provider = new LocalFileStorageProvider(config);
        Assert.Throws<InvalidOperationException>(() => provider.ResolvePath("../outside.txt"));
    }
}
