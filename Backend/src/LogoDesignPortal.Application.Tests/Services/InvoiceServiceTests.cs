using AutoMapper;
using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Configuration;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Mappings;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class InvoiceServiceTests
{
    [Fact]
    public async Task GenerateFlexibleInvoiceAsync_DateRange_CreatesInvoiceAndMarksOrdersInvoiced()
    {
        var (context, client, orders) = await CreateContextWithEligibleOrdersAsync();
        var service = CreateInvoiceService(context);

        var request = new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 31),
            IncludeUninvoicedOnly = true,
            Notes = "Date range invoice"
        };

        var result = await service.GenerateFlexibleInvoiceAsync(request, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(BillingType.Manual, result.BillingType);
        Assert.Equal("2026-01-01 to 2026-01-31", result.BillingPeriod);
        Assert.Equal(2, result.Items.Count);

        var invoicedOrders = await context.LogoOrders.Where(o => o.InvoiceId == result.Id).ToListAsync();
        Assert.Equal(2, invoicedOrders.Count);
        Assert.All(invoicedOrders, o => Assert.True(o.IsInvoiced));
    }

    [Fact]
    public async Task GenerateFlexibleInvoiceAsync_SelectedOrderIds_CreatesInvoiceForSelectedOrdersOnly()
    {
        var (context, client, orders) = await CreateContextWithEligibleOrdersAsync();
        var service = CreateInvoiceService(context);

        var selected = new List<Guid> { orders[0].Id, orders[2].Id };
        var request = new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            SelectedOrderIds = selected,
            IncludeUninvoicedOnly = true,
            Notes = "Custom selection"
        };

        var result = await service.GenerateFlexibleInvoiceAsync(request, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal($"Custom Selection ({selected.Count} Orders)", result.BillingPeriod);
        Assert.Equal(selected.Count, result.OrderIds.Count);
        Assert.All(selected, id => Assert.Contains(id, result.OrderIds));

        var nonSelectedOrder = await context.LogoOrders.FirstAsync(o => o.Id == orders[1].Id);
        Assert.False(nonSelectedOrder.IsInvoiced);
    }

    [Fact]
    public async Task EditInvoiceItemsAsync_RemoveOrderLinkedItem_ResetsOrderInvoiceFlags()
    {
        var (context, client, orders) = await CreateContextWithEligibleOrdersAsync();
        var service = CreateInvoiceService(context);

        var createResult = await service.GenerateFlexibleInvoiceAsync(new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            SelectedOrderIds = new List<Guid> { orders[0].Id, orders[1].Id },
            IncludeUninvoicedOnly = true
        }, Guid.NewGuid());

        var itemToRemove = await context.InvoiceOrders
            .FirstAsync(io => io.InvoiceId == createResult.Id && io.OrderId == orders[0].Id);

        var updated = await service.EditInvoiceItemsAsync(createResult.Id, new EditInvoiceItemsRequestDto
        {
            RemoveItemIds = new List<Guid> { itemToRemove.Id },
            AddManualItems = new List<AddManualInvoiceItemDto>
            {
                new() { Description = "Adjustment", Amount = 25m }
            }
        }, Guid.NewGuid());

        Assert.NotNull(updated);
        Assert.Equal(2, updated.Items.Count); // 1 remaining order item + 1 manual item

        var removedOrder = await context.LogoOrders.FirstAsync(o => o.Id == orders[0].Id);
        Assert.False(removedOrder.IsInvoiced);
        Assert.Null(removedOrder.InvoiceId);

        var keptOrder = await context.LogoOrders.FirstAsync(o => o.Id == orders[1].Id);
        Assert.True(keptOrder.IsInvoiced);
        Assert.Equal(createResult.Id, keptOrder.InvoiceId);
    }

    [Fact]
    public async Task GenerateFlexibleInvoiceAsync_WithoutDateRangeAndSelectedOrders_ThrowsInvalidOperationException()
    {
        var (context, client, _) = await CreateContextWithEligibleOrdersAsync();
        var service = CreateInvoiceService(context);

        var request = new GenerateFlexibleInvoiceRequestDto
        {
            ClientId = client.Id,
            IncludeUninvoicedOnly = true
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateFlexibleInvoiceAsync(request, Guid.NewGuid()));

        Assert.Equal("Either fromDate/toDate or selectedOrderIds must be provided.", ex.Message);
    }

    private static InvoiceService CreateInvoiceService(ApplicationDbContext context)
    {
        return new InvoiceService(
            context,
            CreateMapper(),
            Mock.Of<INotificationService>(),
            Mock.Of<IRealtimeEntityUpdateSender>(),
            Microsoft.Extensions.Options.Options.Create(new ProductionSafetyOptions()),
            Mock.Of<IReadModelCacheVersions>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<InvoiceService>>());
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "InvoiceTests_" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private static async Task<(ApplicationDbContext context, ClientProfile client, List<LogoOrder> orders)> CreateContextWithEligibleOrdersAsync()
    {
        var context = new ApplicationDbContext(CreateInMemoryOptions());

        var clientRole = new Role { Id = Guid.NewGuid(), Name = "Client" };
        var clientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "client@test.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = clientRole.Id,
            Role = clientRole
        };
        var clientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = clientUser.Id,
            CompanyName = "ACME",
            User = clientUser
        };

        var orders = new List<LogoOrder>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientProfile.Id,
                Title = "Order 1",
                Status = OrderStatus.Completed,
                BillingEligible = true,
                IsInvoiced = false,
                Price = 100,
                ClientChargePrice = 100,
                CompletedDate = new DateTime(2026, 1, 5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientProfile.Id,
                Title = "Order 2",
                Status = OrderStatus.Completed,
                BillingEligible = true,
                IsInvoiced = false,
                Price = 150,
                ClientChargePrice = 150,
                CompletedDate = new DateTime(2026, 1, 20)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientProfile.Id,
                Title = "Order 3 old",
                Status = OrderStatus.Completed,
                BillingEligible = true,
                IsInvoiced = false,
                Price = 200,
                ClientChargePrice = 200,
                CompletedDate = new DateTime(2025, 11, 1)
            }
        };

        context.Roles.Add(clientRole);
        context.Users.Add(clientUser);
        context.ClientProfiles.Add(clientProfile);
        context.LogoOrders.AddRange(orders);
        await context.SaveChangesAsync();

        return (context, clientProfile, orders);
    }
}
