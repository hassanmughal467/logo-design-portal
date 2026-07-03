using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogoDesignPortal.Infrastructure.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        var provider = configuration[$"{StorageOptions.SectionName}:Provider"] ?? "Local";
        if (string.Equals(provider, "R2", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IFileStorageService, R2FileStorageService>();
        }
        else
        {
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }
}
