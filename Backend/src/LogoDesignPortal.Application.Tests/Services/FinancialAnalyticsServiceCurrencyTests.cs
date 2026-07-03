using LogoDesignPortal.Application.Caching;
using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

public class FinancialAnalyticsServiceCurrencyTests
{
    private static FinancialAnalyticsService CreateSut(ApplicationDbContext context)
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        return new FinancialAnalyticsService(
            context,
            cache,
            new ReadModelCacheVersions(),
            Mock.Of<ILogger<FinancialAnalyticsService>>());
    }

    [Fact]
    public async Task GetOverviewAsync_UsesGbp_WhenAllCompletedOrdersAreGbp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("FinancialOverview_GBP_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = Guid.NewGuid(),
            CompanyName = "Kelvin Co",
            CurrencyCode = "GBP",
            CreatedAt = DateTime.UtcNow
        });
        context.LogoOrders.Add(new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Title = "Logo",
            Description = "d",
            Status = OrderStatus.Completed,
            Price = 13m,
            CurrencyCode = "GBP",
            ClientBasePrice = 13m,
            ClientChargePrice = 13m,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var overview = await CreateSut(context).GetOverviewAsync();

        Assert.Equal("GBP", overview.RevenueCurrencyCode);
        Assert.False(overview.RevenueCurrencyMixed);
        var rollup = Assert.Single(overview.RevenueByCurrency);
        Assert.Equal("GBP", rollup.CurrencyCode);
        Assert.Equal(13m, rollup.TotalRevenue);
    }

    [Fact]
    public async Task GetOverviewAsync_MarksMixed_WhenCompletedOrdersUseMultipleCurrencies()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("FinancialOverview_Mixed_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var usdClient = Guid.NewGuid();
        var gbpClient = Guid.NewGuid();
        context.ClientProfiles.AddRange(
            new ClientProfile { Id = usdClient, UserId = Guid.NewGuid(), CompanyName = "A", CreatedAt = DateTime.UtcNow },
            new ClientProfile { Id = gbpClient, UserId = Guid.NewGuid(), CompanyName = "B", CurrencyCode = "GBP", CreatedAt = DateTime.UtcNow });
        context.LogoOrders.AddRange(
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = usdClient,
                Title = "U",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 10m,
                CurrencyCode = "USD",
                ClientBasePrice = 10m,
                ClientChargePrice = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = gbpClient,
                Title = "G",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 13m,
                CurrencyCode = "GBP",
                ClientBasePrice = 13m,
                ClientChargePrice = 13m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var overview = await CreateSut(context).GetOverviewAsync();

        Assert.True(overview.RevenueCurrencyMixed);
        Assert.Equal(2, overview.RevenueByCurrency.Count);
        Assert.Contains(overview.RevenueByCurrency, x => x.CurrencyCode == "USD" && x.TotalRevenue == 10m);
        Assert.Contains(overview.RevenueByCurrency, x => x.CurrencyCode == "GBP" && x.TotalRevenue == 13m);
    }

    [Fact]
    public async Task GetClientRevenueAsync_PropagatesClientProfileCurrencyCode()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("FinancialClientRev_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = "gbp@example.com",
            FirstName = "Kevin",
            LastName = "Walker",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = userId,
            CompanyName = "Kelvin Walter",
            CurrencyCode = "GBP",
            CreatedAt = DateTime.UtcNow
        });
        context.LogoOrders.Add(new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Title = "T",
            Description = "d",
            Status = OrderStatus.Completed,
            Price = 13m,
            CurrencyCode = "GBP",
            ClientBasePrice = 13m,
            ClientChargePrice = 13m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var dto = await CreateSut(context).GetClientRevenueAsync();

        var item = Assert.Single(dto.Items);
        Assert.Equal("GBP", item.CurrencyCode);
        Assert.Equal(13m, item.TotalRevenue);
    }
}
