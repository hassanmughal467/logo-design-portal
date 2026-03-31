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
/// Tests CreateOrderAsync, AssignDesignerAsync, RequestPriceApprovalAsync, ApprovePriceAsync, ApproveOrderAsync.
/// Verifies status transitions, price calculations, and authorization rules.
/// </summary>
public class OrderServiceWorkflowTests
{
    [Fact]
    public async Task CreateOrderAsync_ValidInput_SetsStatusWaitingForAdminApproval()
    {
        var (context, clientUserId) = await SeedClientAsync();
        var orderService = CreateOrderService(context);

        var request = new CreateOrderRequestDto
        {
            Title = "Test Logo Order",
            Description = "Need a logo for my company",
            Price = 100,
            DesignCategory = DesignCategory.EmbroideryDigitizing,
            DesignType = DesignType.LeftChest
        };

        var result = await orderService.CreateOrderAsync(request, clientUserId);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.WaitingForAdminApproval.ToString(), result.Status);
        Assert.Equal("Test Logo Order", result.Title);
        Assert.NotEqual(Guid.Empty, result.Id);

        var savedOrder = await context.LogoOrders.FirstOrDefaultAsync(o => o.Id == result.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(OrderStatus.WaitingForAdminApproval, savedOrder.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_InvalidClient_ThrowsInvalidOperationException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "CreateOrder_Invalid_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);
        var orderService = CreateOrderService(context);

        var request = new CreateOrderRequestDto { Title = "Test", Description = "Desc", Price = 50 };
        var nonExistentClientId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(request, nonExistentClientId));
    }

    [Fact]
    public async Task AssignDesignerAsync_ValidInput_SetsDesignerAndStatusInProgress()
    {
        var (context, orderId, designerUserId, adminUserId) = await SeedOrderWithDesignerAsync();
        var orderService = CreateOrderService(context);

        var result = await orderService.AssignOrderToDesignerAsync(orderId, designerUserId, adminUserId);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.InProgress.ToString(), result.Status);
        Assert.NotNull(result.Designer);

        var savedOrder = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(savedOrder);
        Assert.NotNull(savedOrder!.DesignerId);
        Assert.Equal(OrderStatus.InProgress, savedOrder.Status);
    }

    [Fact]
    public async Task AssignDesignerAsync_OrderNotFound_ThrowsInvalidOperationException()
    {
        var (context, _, designerUserId, adminUserId) = await SeedOrderWithDesignerAsync();
        var orderService = CreateOrderService(context);
        var nonExistentOrderId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.AssignOrderToDesignerAsync(nonExistentOrderId, designerUserId, adminUserId));
    }

    [Fact]
    public async Task AssignDesignerAsync_CancelledOrder_ThrowsInvalidOperationException()
    {
        var (context, orderId, designerUserId, adminUserId) = await SeedOrderWithDesignerAsync(OrderStatus.Cancelled);
        var orderService = CreateOrderService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.AssignOrderToDesignerAsync(orderId, designerUserId, adminUserId));
    }

    [Fact]
    public async Task AssignDesignerAsync_WhilePriceApprovalPending_KeepsStatusAndRecordsCorrectHistory()
    {
        var (context, orderId, designerUserId, adminUserId) = await SeedOrderWithDesignerAsync(OrderStatus.PriceApprovalPending);
        var orderService = CreateOrderService(context);

        var result = await orderService.AssignOrderToDesignerAsync(orderId, designerUserId, adminUserId);

        Assert.Equal(OrderStatus.PriceApprovalPending.ToString(), result.Status);
        var savedOrder = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(savedOrder);
        Assert.NotNull(savedOrder!.DesignerId);
        Assert.Equal(OrderStatus.PriceApprovalPending, savedOrder.Status);

        var lastHistory = await context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.CreatedAt)
            .FirstAsync();
        Assert.Equal(OrderStatus.PriceApprovalPending, lastHistory.NewStatus);
        Assert.Equal(OrderStatus.PriceApprovalPending, lastHistory.PreviousStatus);
    }

    [Fact]
    public async Task RequestPriceApprovalAsync_ValidInput_SetsStatusPriceApprovalPending()
    {
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.WaitingForAdminApproval, useAdmin: true);
        var orderService = CreateOrderService(context);

        var request = new RequestPriceApprovalDto { ProposedPrice = 150, Notes = "Custom pricing" };

        var result = await orderService.RequestPriceApprovalAsync(orderId, request, adminUserId);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.PriceApprovalPending.ToString(), result.Status);

        var savedOrder = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(savedOrder);
        Assert.Equal(OrderStatus.PriceApprovalPending, savedOrder!.Status);
        Assert.True(savedOrder.RequiresPriceApproval);
        Assert.Equal(150, savedOrder.ClientPrice);
    }

    [Fact]
    public async Task ApprovePriceAsync_AsClient_SetsClientChargePriceAndStatus()
    {
        var (context, orderId, clientUserId) = await SeedOrderAsync(OrderStatus.PriceApprovalPending);
        var order = await context.LogoOrders.FindAsync(orderId);
        order!.ClientPrice = 200;
        await context.SaveChangesAsync();

        var orderService = CreateOrderService(context);
        var request = new ApprovePriceDto { Approved = true, Comment = "Looks good" };

        var result = await orderService.ApprovePriceAsync(orderId, request, clientUserId);

        Assert.NotNull(result);
        Assert.Contains(result.Status, new[] { OrderStatus.WaitingForAdminApproval.ToString(), OrderStatus.InProgress.ToString() });

        var savedOrder = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(savedOrder);
        Assert.Equal(200, savedOrder!.ClientChargePrice);
        Assert.True(savedOrder.PriceApproved);
    }

    [Fact]
    public async Task ApprovePriceAsync_NonClient_ThrowsForbiddenAccessException()
    {
        var (context, orderId, clientUserId) = await SeedOrderAsync(OrderStatus.PriceApprovalPending);
        var orderService = CreateOrderService(context);
        var request = new ApprovePriceDto { Approved = true };
        var nonClientUserId = Guid.NewGuid();

        await Assert.ThrowsAsync<LogoDesignPortal.Application.Exceptions.ForbiddenAccessException>(
            () => orderService.ApprovePriceAsync(orderId, request, nonClientUserId));
    }

    [Fact]
    public async Task ApproveOrderAsync_ValidInput_SetsStatusInProgress()
    {
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.WaitingForAdminApproval, useAdmin: true);
        var orderService = CreateOrderService(context);

        var result = await orderService.ApproveOrderAsync(orderId, adminUserId);

        Assert.NotNull(result);
        Assert.Equal(OrderStatus.InProgress.ToString(), result.Status);

        var savedOrder = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(savedOrder);
        Assert.Equal(OrderStatus.InProgress, savedOrder!.Status);
    }

    [Fact]
    public async Task ApproveOrderAsync_NotWaitingForAdminApproval_ThrowsInvalidOperationException()
    {
        var (context, orderId, adminUserId) = await SeedOrderAsync(OrderStatus.InProgress, useAdmin: true);
        var orderService = CreateOrderService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.ApproveOrderAsync(orderId, adminUserId));
    }

    private static async Task<(ApplicationDbContext context, Guid clientUserId)> SeedClientAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderWorkflow_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientUserId = Guid.NewGuid();
        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test Co", User = clientUser };

        context.Roles.Add(clientRole);
        context.Users.Add(clientUser);
        context.ClientProfiles.Add(clientProfile);
        await context.SaveChangesAsync();

        return (context, clientUserId);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid userId)> SeedOrderAsync(
        OrderStatus status, bool useAdmin = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderWorkflow_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
        var clientUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var adminUser = new User { Id = adminUserId, Email = "admin@test.com", FirstName = "A", LastName = "Admin", PasswordHash = "h", RoleId = adminRole.Id, Role = adminRole };
        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };

        var orderId = Guid.NewGuid();
        var order = new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfile.Id,
            DesignerId = null,
            Title = "Test",
            Status = status,
            Price = 100,
            Client = clientProfile
        };

        context.Roles.AddRange(clientRole, adminRole);
        context.Users.AddRange(clientUser, adminUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        var userId = useAdmin ? adminUserId : clientUserId;
        return (context, orderId, userId);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid designerUserId, Guid adminUserId)> SeedOrderWithDesignerAsync(
        OrderStatus status = OrderStatus.WaitingForAdminApproval)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderWorkflow_" + Guid.NewGuid())
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
            DesignerId = null,
            Title = "Test",
            Status = status,
            Price = 100,
            Client = clientProfile
        };

        context.Roles.AddRange(clientRole, designerRole, adminRole);
        context.Users.AddRange(clientUser, designerUser, adminUser);
        context.ClientProfiles.Add(clientProfile);
        context.DesignerProfiles.Add(designerProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return (context, orderId, designerUserId, adminUserId);
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
