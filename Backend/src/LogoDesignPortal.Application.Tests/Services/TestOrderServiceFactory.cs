using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Tests.Storage;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LogoDesignPortal.Application.Tests.Services;

internal static class TestOrderServiceFactory
{
    public static OrderService Create(
        ApplicationDbContext context,
        INotificationService? notificationService = null,
        string? storageRoot = null)
    {
        var root = storageRoot ?? Path.Combine(Path.GetTempPath(), "ldp-order-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(root);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Path"] = root })
            .Build();

        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        return new OrderService(
            context,
            mapper,
            notificationService ?? Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Mock.Of<IFileService>(),
            Mock.Of<IClientLogoPricingService>(),
            Mock.Of<ICommentService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderService>>(),
            Mock.Of<IDistributedCache>(),
            Mock.Of<IReadModelCacheVersions>(),
            new ClientProfileEnsureService(context, Mock.Of<IReadModelCacheVersions>(), Mock.Of<Microsoft.Extensions.Logging.ILogger<ClientProfileEnsureService>>()),
            TestFileStorageFactory.CreateLocal(root),
            config,
            TestFileStorageFactory.CreateLocalOptions(root));
    }
}
