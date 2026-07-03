using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Storage;
using LogoDesignPortal.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Tests.Storage;

public static class TestFileStorageFactory
{
    public static IFileStorageService CreateLocal(string basePath)
    {
        var options = Options.Create(new StorageOptions
        {
            Provider = "Local",
            Local = new LocalStorageOptions { BasePath = basePath }
        });
        return new LocalFileStorageService(options, NullLogger<LocalFileStorageService>.Instance);
    }

    public static IOptions<StorageOptions> CreateLocalOptions(string basePath) =>
        Options.Create(new StorageOptions
        {
            Provider = "Local",
            Local = new LocalStorageOptions { BasePath = basePath }
        });
}
