using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Regression tests: orders and invoices must stay visible to admins after the owning client
/// profile is soft-deleted. EF Core global query filters on ClientProfile turn Include(Client)
/// into an inner join, which silently dropped these rows (paged orders returned Total > 0 with
/// empty Items). Services use IgnoreQueryFilters + explicit IsDeleted checks to avoid this.
/// </summary>
public class SoftDeletedClientVisibilityTests
{
    [Fact]
    public async Task GetOrdersPagedAsync_ClientSoftDeleted_OrderStillListed()
    {
        var (context, orderId, _) = await SeedOrderWithSoftDeletedClientAsync();
        var service = TestOrderServiceFactory.Create(context);

        var result = await service.GetOrdersPagedAsync("SuperAdmin", page: 1, pageSize: 50);

        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal(orderId, result.Items[0].Id);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ClientSoftDeleted_OrderStillListed()
    {
        var (context, orderId, _) = await SeedOrderWithSoftDeletedClientAsync();
        var service = TestOrderServiceFactory.Create(context);

        var result = await service.GetAllOrdersAsync("SuperAdmin");

        Assert.Single(result);
        Assert.Equal(orderId, result[0].Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ClientSoftDeleted_AdminCanStillLoadOrder()
    {
        var (context, orderId, adminUserId) = await SeedOrderWithSoftDeletedClientAsync();
        var service = TestOrderServiceFactory.Create(context);

        var result = await service.GetOrderByIdAsync(orderId, adminUserId, "SuperAdmin");

        Assert.NotNull(result);
        Assert.Equal(orderId, result!.Id);
    }

    [Fact]
    public async Task GetOrdersPagedAsync_DeletedOrder_NotListed()
    {
        var (context, orderId, _) = await SeedOrderWithSoftDeletedClientAsync();
        var order = await context.LogoOrders.IgnoreQueryFilters().FirstAsync(o => o.Id == orderId);
        order.IsDeleted = true;
        await context.SaveChangesAsync();
        var service = TestOrderServiceFactory.Create(context);

        var result = await service.GetOrdersPagedAsync("SuperAdmin", page: 1, pageSize: 50);

        Assert.Equal(0, result.Total);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetInvoicesAsync_ClientSoftDeleted_InvoiceStillListedForAdmin()
    {
        var (context, _, clientProfileId) = await SeedInvoiceContextAsync();

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-0001",
            ClientId = clientProfileId,
            Status = InvoiceStatus.Pending,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            Amount = 100m,
            TotalAmount = 100m,
            CreatedAt = DateTime.UtcNow
        };
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        var invoiceService = new InvoiceService(
            context,
            new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Options.Create(new ProductionSafetyOptions()),
            Mock.Of<IReadModelCacheVersions>(),
            Mock.Of<ICurrencyService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<InvoiceService>>());

        var result = await invoiceService.GetInvoicesAsync(null, "SuperAdmin");

        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal(invoice.Id, result.Items[0].Id);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid adminUserId)> SeedOrderWithSoftDeletedClientAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("SoftDeletedClient_" + Guid.NewGuid())
            .Options;
        var context = new ApplicationDbContext(options);

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "SuperAdmin" };
        var clientUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var clientUser = new User
        {
            Id = clientUserId,
            Email = "deleted-client@test.com",
            FirstName = "Del",
            LastName = "Client",
            PasswordHash = "h",
            RoleId = clientRole.Id,
            Role = clientRole,
            IsDeleted = true
        };
        var adminUser = new User
        {
            Id = adminUserId,
            Email = "admin@test.com",
            FirstName = "A",
            LastName = "Admin",
            PasswordHash = "h",
            RoleId = adminRole.Id,
            Role = adminRole
        };

        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = clientUserId,
            CompanyName = "Deleted Co",
            User = clientUser,
            IsDeleted = true
        };

        var orderId = Guid.NewGuid();
        var order = new LogoOrder
        {
            Id = orderId,
            ClientId = clientProfile.Id,
            Title = "Order from deleted client",
            Description = "Order must remain visible to admins after client removal.",
            Status = OrderStatus.Completed,
            Price = 100,
            Client = clientProfile,
            CreatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(clientRole, adminRole);
        context.Users.AddRange(clientUser, adminUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();

        return (context, orderId, adminUserId);
    }

    private static async Task<(ApplicationDbContext context, Guid orderId, Guid clientProfileId)> SeedInvoiceContextAsync()
    {
        var (context, orderId, _) = await SeedOrderWithSoftDeletedClientAsync();
        var order = await context.LogoOrders.IgnoreQueryFilters().FirstAsync(o => o.Id == orderId);
        return (context, orderId, order.ClientId);
    }
}
