using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Storage;

public class MigrateLocalFilesToR2Tests
{
    [Fact]
    public async Task MigrateAsync_UploadsNewFilesAndSkipsExisting()
    {
        var sourceRoot = Path.Combine(Path.GetTempPath(), "ldp-migrate-src-" + Guid.NewGuid());
        var targetRoot = Path.Combine(Path.GetTempPath(), "ldp-migrate-r2-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(sourceRoot, "Permanent"));
        Directory.CreateDirectory(Path.Combine(sourceRoot, "Temporary"));
        await File.WriteAllTextAsync(Path.Combine(sourceRoot, "Permanent", "logo.png"), "png-bytes");
        await File.WriteAllTextAsync(Path.Combine(sourceRoot, "Temporary", "preview.png"), "preview");

        var r2 = TestFileStorageFactory.CreateLocal(targetRoot);
        await using (var existing = new MemoryStream("existing"u8.ToArray()))
        {
            await r2.UploadAsync(existing, "Permanent/logo.png", "image/png");
        }

        var migrator = new MigrateLocalFilesToR2();
        await migrator.MigrateAsync(sourceRoot, r2, NullLogger.Instance);

        Assert.True(await r2.ExistsAsync("Permanent/logo.png"));
        Assert.True(await r2.ExistsAsync("Temporary/preview.png"));

        await using var downloaded = await r2.DownloadAsync("Temporary/preview.png");
        using var reader = new StreamReader(downloaded);
        Assert.Equal("preview", await reader.ReadToEndAsync());

        await using var kept = await r2.DownloadAsync("Permanent/logo.png");
        using var keptReader = new StreamReader(kept);
        Assert.Equal("existing", await keptReader.ReadToEndAsync());
    }

    [Fact]
    public async Task MigrateAsync_WhenSourceMissing_DoesNotThrow()
    {
        var missing = Path.Combine(Path.GetTempPath(), "ldp-migrate-missing-" + Guid.NewGuid());
        var targetRoot = Path.Combine(Path.GetTempPath(), "ldp-migrate-empty-" + Guid.NewGuid());
        var r2 = TestFileStorageFactory.CreateLocal(targetRoot);

        var migrator = new MigrateLocalFilesToR2();
        await migrator.MigrateAsync(missing, r2, NullLogger.Instance);

        var keys = await r2.ListKeysAsync(string.Empty);
        Assert.Empty(keys);
    }
}
