using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using AutoMapper;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Verifies rule 4: Admin always sends full preview batches.
/// Backend enforces: partial delivery of preview files is rejected.
/// </summary>
public class OrderServiceFullBatchTests
{
    [Fact]
    public async Task SendFilesToClient_WithPartialPreviewBatch_ThrowsInvalidOperationException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FullBatch_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        SeedDatabase(context, out var orderId, out var batchId, out var fileIds);

        var orderService = CreateOrderService(context);

        // Admin tries to send only 1 of 2 preview files in the batch
        var partialFileIds = new List<Guid> { fileIds[0] };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.SendFilesToClientAsync(orderId, partialFileIds, Guid.NewGuid()));

        Assert.Contains("entire preview batch", ex.Message);
        Assert.Contains("Partial delivery", ex.Message);
    }

    [Fact]
    public async Task SendFilesToClient_WithFullPreviewBatch_Succeeds()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FullBatch_Success_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        SeedDatabase(context, out var orderId, out var batchId, out var fileIds);

        var orderService = CreateOrderService(context);

        // Admin sends all files in the batch
        var result = await orderService.SendFilesToClientAsync(orderId, fileIds, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.PreviewDelivered.ToString(), result.Status);
    }

    private static void SeedDatabase(ApplicationDbContext context, out Guid orderId, out Guid batchId, out List<Guid> fileIds)
    {
        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };

        var clientUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var clientUser = new User
        {
            Id = clientUserId,
            Email = "client@test.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };

        var designerUser = new User
        {
            Id = designerUserId,
            Email = "designer@test.com",
            FirstName = "Designer",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = designerRole.Id,
            Role = designerRole
        };

        var adminUser = new User
        {
            Id = adminUserId,
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = "hash",
            RoleId = adminRole.Id,
            Role = adminRole
        };

        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = clientUserId,
            CompanyName = "Test Co",
            User = clientUser
        };

        var designerProfile = new DesignerProfile
        {
            Id = Guid.NewGuid(),
            UserId = designerUserId,
            User = designerUser
        };

        orderId = Guid.NewGuid();
        batchId = Guid.NewGuid();

        var order = new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfile.Id,
            DesignerId = designerProfile.Id,
            Title = "Test",
            Description = "Test",
            Status = OrderStatus.InProgress,
            Price = 100,
            Client = clientProfile,
            Designer = designerProfile
        };

        fileIds = new List<Guid>();
        for (int i = 0; i < 2; i++)
        {
            var file = new LogoFile
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                FileName = $"preview{i}.png",
                OriginalFileName = $"preview{i}.png",
                FilePath = $"/tmp/preview{i}.png",
                ContentType = "image/png",
                FileSize = 100,
                FileType = FileType.Preview,
                IsVisibleToClient = false,
                IsAdminApproved = false,
                VersionNumber = i + 1,
                UploadedBy = designerUserId,
                PreviewBatchId = batchId
            };
            fileIds.Add(file.Id);
            context.LogoFiles.Add(file);
        }

        context.Roles.AddRange(clientRole, adminRole, designerRole);
        context.Users.AddRange(clientUser, designerUser, adminUser);
        context.ClientProfiles.Add(clientProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.Add(order);
        context.SaveChanges();
    }

    private static OrderService CreateOrderService(ApplicationDbContext context)
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<LogoDesignPortal.Application.Mappings.MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        return new OrderService(
            context,
            mapper,
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Mock.Of<IFileService>(),
            Mock.Of<IClientLogoPricingService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderService>>());
    }
}
