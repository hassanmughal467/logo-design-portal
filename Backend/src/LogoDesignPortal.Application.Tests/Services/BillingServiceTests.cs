using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LogoDesignPortal.Application.DTOs.Billing;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Tests BillingService: GetBillingQueueOverviewAsync, GetEligibleOrdersForClientAsync, CreateInvoiceFromOrdersAsync.
/// Verifies billing eligibility logic and invoice generation.
/// </summary>
public class BillingServiceTests
{
    [Fact]
    public async Task GetBillingQueueOverviewAsync_NoEligibleOrders_ReturnsEmpty()
    {
        var context = await CreateContextWithOrdersAsync(OrderStatus.InProgress);
        var billingService = CreateBillingService(context);

        var result = await billingService.GetBillingQueueOverviewAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBillingQueueOverviewAsync_CompletedBillingEligibleOrders_ReturnsClients()
    {
        var (context, clientProfile) = await CreateContextWithCompletedOrderAsync();
        var billingService = CreateBillingService(context);

        var result = await billingService.GetBillingQueueOverviewAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(clientProfile.Id, result[0].ClientId);
        Assert.Equal(1, result[0].UninvoicedOrderCount);
        Assert.True(result[0].TotalPendingAmount > 0);
    }

    [Fact]
    public async Task GetEligibleOrdersForClientAsync_ValidClient_ReturnsEligibleOrders()
    {
        var (context, clientProfile) = await CreateContextWithCompletedOrderAsync();
        var billingService = CreateBillingService(context);

        var result = await billingService.GetEligibleOrdersForClientAsync(clientProfile.Id);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100, result[0].Price);
    }

    [Fact]
    public async Task GetEligibleOrdersForClientAsync_InvalidClient_ReturnsEmpty()
    {
        var context = await CreateContextWithOrdersAsync(OrderStatus.InProgress);
        var billingService = CreateBillingService(context);

        var result = await billingService.GetEligibleOrdersForClientAsync(Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateInvoiceFromOrdersAsync_ValidOrderIds_CreatesInvoice()
    {
        var (context, clientProfile, orderId) = await CreateContextWithCompletedOrderForInvoiceAsync();
        var billingService = CreateBillingService(context);

        var result = await billingService.CreateInvoiceFromOrdersAsync(
            clientProfile.Id,
            null,
            new List<Guid> { orderId },
            "March 2026",
            Guid.NewGuid());

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        var invoiceOrder = await context.InvoiceOrders.FirstOrDefaultAsync(io => io.OrderId == orderId);
        Assert.NotNull(invoiceOrder);
        Assert.Equal(100, invoiceOrder.Amount);

        var order = await context.LogoOrders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.True(order!.IsInvoiced);
    }

    [Fact]
    public async Task CreateInvoiceFromOrdersAsync_WithEditablePrices_UsesProvidedPrices()
    {
        var (context, clientProfile, orderId) = await CreateContextWithCompletedOrderForInvoiceAsync();
        var billingService = CreateBillingService(context);

        var orders = new List<CreateInvoiceOrderItemDto> { new() { OrderId = orderId, Price = 150 } };
        var result = await billingService.CreateInvoiceFromOrdersAsync(
            clientProfile.Id,
            orders,
            null,
            "March 2026",
            Guid.NewGuid());

        Assert.NotNull(result);

        var invoiceOrder = await context.InvoiceOrders.FirstOrDefaultAsync(io => io.OrderId == orderId);
        Assert.NotNull(invoiceOrder);
        Assert.Equal(150, invoiceOrder!.Amount);
    }

    [Fact]
    public async Task CreateInvoiceFromOrdersAsync_ClientNotFound_ThrowsInvalidOperationException()
    {
        var context = await CreateContextWithOrdersAsync(OrderStatus.InProgress);
        var billingService = CreateBillingService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => billingService.CreateInvoiceFromOrdersAsync(
                Guid.NewGuid(),
                null,
                new List<Guid> { Guid.NewGuid() },
                null,
                Guid.NewGuid()));
    }

    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Billing_" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private static async Task<ApplicationDbContext> CreateContextWithOrdersAsync(OrderStatus status)
    {
        var options = CreateInMemoryOptions();
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientUserId = Guid.NewGuid();
        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };

        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfile.Id,
            Title = "Test",
            Status = status,
            Price = 100,
            ClientChargePrice = 100,
            BillingEligible = status == OrderStatus.Completed
        };

        context.Roles.Add(clientRole);
        context.Users.Add(clientUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return context;
    }

    private static async Task<(ApplicationDbContext context, ClientProfile clientProfile)> CreateContextWithCompletedOrderAsync()
    {
        var options = CreateInMemoryOptions();
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientUserId = Guid.NewGuid();
        var clientUser = new User { Id = clientUserId, Email = "client@test.com", FirstName = "C", LastName = "Client", PasswordHash = "h", RoleId = clientRole.Id, Role = clientRole };
        var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = clientUserId, CompanyName = "Test", User = clientUser };

        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfile.Id,
            Title = "Test",
            Status = OrderStatus.Completed,
            Price = 100,
            ClientChargePrice = 100,
            BillingEligible = true,
            IsInvoiced = false
        };

        context.Roles.Add(clientRole);
        context.Users.Add(clientUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return (context, clientProfile);
    }

    private static async Task<(ApplicationDbContext context, ClientProfile clientProfile, Guid orderId)> CreateContextWithCompletedOrderForInvoiceAsync()
    {
        var (context, clientProfile) = await CreateContextWithCompletedOrderAsync();
        var order = await context.LogoOrders.FirstAsync(o => o.ClientId == clientProfile.Id);
        return (context, clientProfile, order.Id);
    }

    private static BillingService CreateBillingService(ApplicationDbContext context)
    {
        var invoiceService = new InvoiceService(
            context,
            CreateMapper(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Microsoft.Extensions.Options.Options.Create(new ProductionSafetyOptions()),
            Mock.Of<LogoDesignPortal.Application.Caching.IReadModelCacheVersions>(),
            Mock.Of<ICurrencyService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<InvoiceService>>());

        var safetyOptions = new ProductionSafetyOptions { DisableBillingGeneration = false };
        return new BillingService(
            context,
            invoiceService,
            Options.Create(safetyOptions),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<BillingService>>());
    }

    private static AutoMapper.IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<LogoDesignPortal.Application.Mappings.MappingProfile>());
        return config.CreateMapper();
    }
}
