using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// TEST-003: Verifies status transition rules via UpdateOrderStatusAsync - Client/Designer/Admin allowed transitions.
/// </summary>
public class OrderServiceStatusTransitionTests
{
    [Fact]
    public async Task UpdateOrderStatus_ClientTriesInvalidTransitionFromPreviewDelivered_ThrowsInvalidOperationException()
    {
        var (context, orderId, clientUserId) = await SeedOrderAsync(OrderStatus.PreviewDelivered);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.InProgress.ToString() };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.UpdateOrderStatusAsync(orderId, request, clientUserId, "Client"));

        Assert.Contains("don't have permission", ex.Message);
    }

    [Fact]
    public async Task UpdateOrderStatus_ClientTransitionsToRevisionRequestedFromPreviewDelivered_Succeeds()
    {
        var (context, orderId, clientUserId) = await SeedOrderAsync(OrderStatus.PreviewDelivered);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.RevisionRequested.ToString() };

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, clientUserId, "Client");

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.RevisionRequested.ToString(), result.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_ClientTransitionsToClientApprovedFromPreviewDelivered_Succeeds()
    {
        var (context, orderId, clientUserId) = await SeedOrderAsync(OrderStatus.PreviewDelivered);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.ClientApproved.ToString() };

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, clientUserId, "Client");

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.ClientApproved.ToString(), result.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_DesignerTriesInvalidTransitionFromInProgress_ThrowsInvalidOperationException()
    {
        var (context, orderId, designerUserId) = await SeedOrderAsync(OrderStatus.InProgress, assignDesigner: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.Completed.ToString() };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.UpdateOrderStatusAsync(orderId, request, designerUserId, "Designer"));

        Assert.Contains("don't have permission", ex.Message);
    }

    [Fact]
    public async Task UpdateOrderStatus_DesignerCannotChangeStatus_ThrowsInvalidOperationException()
    {
        // Designers cannot change order status via UpdateOrderStatus - upload preview files only
        var (context, orderId, designerUserId) = await SeedOrderAsync(OrderStatus.InProgress, assignDesigner: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.PreviewDelivered.ToString() };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.UpdateOrderStatusAsync(orderId, request, designerUserId, "Designer"));

        Assert.Contains("don't have permission", ex.Message);
    }

    [Fact]
    public async Task UpdateOrderStatus_AdminCanTransitionToAnyStatus_Succeeds()
    {
        // State machine allows ClientApproved -> Completed; InProgress cannot go directly to Completed
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.ClientApproved, useAdmin: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.Completed.ToString() };

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, adminUserId, "Admin");

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Completed.ToString(), result.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_AdminFromRevisionRequested_CannotSetInProgress_ThrowsInvalidOperationException()
    {
        // Revision workflow: Admin must NOT move RevisionRequested -> InProgress
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.RevisionRequested, assignDesigner: true, useAdmin: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.InProgress.ToString() };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.UpdateOrderStatusAsync(orderId, request, adminUserId, "Admin"));

        Assert.Contains("don't have permission", ex.Message);
    }

    [Fact]
    public async Task UpdateOrderStatus_AdminFromRevisionRequested_CanSetPreviewDelivered_Succeeds()
    {
        // Revision workflow: Admin sends revised preview -> PreviewDelivered
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.RevisionRequested, assignDesigner: true, useAdmin: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.PreviewDelivered.ToString() };

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, adminUserId, "Admin");

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.PreviewDelivered.ToString(), result.Status);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid userId)> SeedOrderAsync(
        OrderStatus status, bool assignDesigner = false, bool useAdmin = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "StatusTransition_" + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        var clientUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var designerUser = new User { Id = designerUserId, Email = "designer@test.com", FirstName = "D", LastName = "Designer", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole };
        var adminUser = new User { Id = adminUserId, Email = "admin@test.com", FirstName = "A", LastName = "Admin", PasswordHash = "h", RoleId = adminRole.Id, Role = adminRole };

        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };
        var designerProfile = new DesignerProfile { Id = Guid.NewGuid(), UserId = designerUserId, User = designerUser };

        var orderId = Guid.NewGuid();
        var order = new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfile.Id,
            DesignerId = assignDesigner ? designerProfile.Id : null,
            Title = "Test",
            Status = status,
            Price = 100,
            Client = clientProfile,
            Designer = assignDesigner ? designerProfile : null
        };

        context.Roles.AddRange(clientRole, designerRole, adminRole);
        context.Users.AddRange(clientUser, designerUser, adminUser);
        context.ClientProfiles.Add(clientProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        var userId = useAdmin ? adminUserId : (assignDesigner ? designerUserId : clientUserId);
        return (context, orderId, userId);
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
            Mock.Of<ICommentService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<OrderService>>(),
            Mock.Of<IDistributedCache>(),
            Mock.Of<IReadModelCacheVersions>(),
            new ClientProfileEnsureService(context, Mock.Of<IReadModelCacheVersions>(), Mock.Of<Microsoft.Extensions.Logging.ILogger<ClientProfileEnsureService>>()));
    }
}
