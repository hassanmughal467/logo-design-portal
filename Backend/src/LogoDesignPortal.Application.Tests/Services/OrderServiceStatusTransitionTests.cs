using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Exceptions;
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
    public async Task UpdateOrderStatus_ClientCannotUpdateAnotherClientsOrderStatus_ThrowsForbiddenAccessException()
    {
        var (context, orderId, ownerClientUserId) = await SeedOrderAsync(OrderStatus.PreviewDelivered);
        var otherClientUserId = await GetOtherClientUserIdAsync(context, ownerClientUserId);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.ClientApproved.ToString() };

        var ex = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => orderService.UpdateOrderStatusAsync(orderId, request, otherClientUserId, "Client"));

        Assert.Contains("don't have access", ex.Message);
        var savedOrder = await context.LogoOrders.SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.PreviewDelivered, savedOrder.Status);
        Assert.False(await context.OrderStatusHistories.AnyAsync(h => h.OrderId == orderId));
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
    public async Task UpdateOrderStatus_SuperAdminCanTransitionToAllowedStatus_Succeeds()
    {
        var (context, orderId, superAdminUserId) = await SeedOrderAsync(OrderStatus.ClientApproved, useSuperAdmin: true);
        var orderService = CreateOrderService(context);

        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.Completed.ToString() };

        var result = await orderService.UpdateOrderStatusAsync(orderId, request, superAdminUserId, "SuperAdmin");

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
        OrderStatus status, bool assignDesigner = false, bool useAdmin = false, bool useSuperAdmin = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "StatusTransition_" + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var designerRole = new Role { Id = Guid.NewGuid(), Name = "Designer" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var superAdminRole = new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" };

        var clientUserId = Guid.NewGuid();
        var otherClientUserId = Guid.NewGuid();
        var designerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var superAdminUserId = Guid.NewGuid();

        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var otherClientUser = new User { Id = otherClientUserId, Email = "other-client@test.com", FirstName = "O", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var designerUser = new User { Id = designerUserId, Email = "designer@test.com", FirstName = "D", LastName = "Designer", PasswordHash = "h", RoleId = designerRole.Id, Role = designerRole };
        var adminUser = new User { Id = adminUserId, Email = "admin@test.com", FirstName = "A", LastName = "Admin", PasswordHash = "h", RoleId = adminRole.Id, Role = adminRole };
        var superAdminUser = new User { Id = superAdminUserId, Email = "superadmin@test.com", FirstName = "S", LastName = "Admin", PasswordHash = "h", RoleId = superAdminRole.Id, Role = superAdminRole };

        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };
        var otherClientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = otherClientUserId, CompanyName = "Other Test", User = otherClientUser };
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

        context.Roles.AddRange(clientRole, designerRole, adminRole, superAdminRole);
        context.Users.AddRange(clientUser, otherClientUser, designerUser, adminUser, superAdminUser);
        context.ClientProfiles.AddRange(clientProfile, otherClientProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        var userId = useSuperAdmin ? superAdminUserId : useAdmin ? adminUserId : (assignDesigner ? designerUserId : clientUserId);
        return (context, orderId, userId);
    }

    private static Task<Guid> GetOtherClientUserIdAsync(ApplicationDbContext context, Guid ownerClientUserId)
    {
        return context.ClientProfiles
            .Where(c => c.UserId != ownerClientUserId)
            .Select(c => c.UserId)
            .SingleAsync();
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
            Mock.Of<IReadModelCacheVersions>());
    }
}
