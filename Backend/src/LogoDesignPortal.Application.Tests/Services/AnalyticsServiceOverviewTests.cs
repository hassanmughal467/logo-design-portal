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

public class AnalyticsServiceOverviewTests
{
    [Fact]
    public async Task GetOverviewAsync_ReturnsAggregatedCounts_FromSeededOrders()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AnalyticsOverview_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = Guid.NewGuid(),
            CompanyName = "Test Co",
            CreatedAt = DateTime.UtcNow
        });

        context.LogoOrders.AddRange(
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "A",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 100m,
                ClientBasePrice = 0,
                ClientChargePrice = 100m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "B",
                Description = "d",
                Status = OrderStatus.InProgress,
                Price = 50m,
                ClientBasePrice = 0,
                ClientChargePrice = 50m,
                CreatedAt = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(-1)
            });
        await context.SaveChangesAsync();

        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var sut = new AnalyticsService(
            context,
            cache,
            new ReadModelCacheVersions(),
            Mock.Of<ILogger<AnalyticsService>>());

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(2, overview.TotalOrders);
        Assert.Equal(1, overview.OrdersInProgress);
        Assert.Equal(1, overview.OverdueOrders);
        Assert.Equal(100m, overview.TotalRevenue);
        Assert.Equal("USD", overview.RevenueCurrencyCode);
        Assert.False(overview.RevenueCurrencyMixed);

        var rollup = Assert.Single(overview.RevenueByCurrency);
        Assert.Equal("USD", rollup.CurrencyCode);
        Assert.Equal(100m, rollup.TotalRevenue);
        Assert.Equal(1, rollup.CompletedCount);
        Assert.Equal(100m, rollup.AverageOrderValue);
    }

    [Fact]
    public async Task GetOverviewAsync_UsesGbpCurrencyCode_WhenAllCompletedOrdersAreGbp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AnalyticsOverview_GBP_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = Guid.NewGuid(),
            CompanyName = "Kelvin Co",
            CreatedAt = DateTime.UtcNow
        });

        context.LogoOrders.AddRange(
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "Logo 1",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 3m,
                CurrencyCode = "GBP",
                ClientBasePrice = 3m,
                ClientChargePrice = 3m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "Logo 2",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 6m,
                CurrencyCode = "GBP",
                ClientBasePrice = 6m,
                ClientChargePrice = 6m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var sut = new AnalyticsService(
            context,
            cache,
            new ReadModelCacheVersions(),
            Mock.Of<ILogger<AnalyticsService>>());

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(9m, overview.TotalRevenue);
        Assert.Equal("GBP", overview.RevenueCurrencyCode);
        Assert.False(overview.RevenueCurrencyMixed);

        var rollup = Assert.Single(overview.RevenueByCurrency);
        Assert.Equal("GBP", rollup.CurrencyCode);
        Assert.Equal(9m, rollup.TotalRevenue);
        Assert.Equal(2, rollup.CompletedCount);
        Assert.Equal(4.5m, rollup.AverageOrderValue);
    }

    [Fact]
    public async Task GetOverviewAsync_PopulatesPerCurrencyRollup_WhenCompletedOrdersHaveMixedCurrencies()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AnalyticsOverview_Mixed_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = Guid.NewGuid(),
            CompanyName = "Mixed Co",
            CreatedAt = DateTime.UtcNow
        });

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        context.LogoOrders.AddRange(
            // Two GBP orders (10 + 20 = 30 total; 20 within current month)
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "GBP-1",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 10m,
                CurrencyCode = "GBP",
                ClientBasePrice = 10m,
                ClientChargePrice = 10m,
                CreatedAt = startOfMonth.AddMonths(-2),
                UpdatedAt = startOfMonth.AddMonths(-2)
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "GBP-2",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 20m,
                CurrencyCode = "GBP",
                ClientBasePrice = 20m,
                ClientChargePrice = 20m,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now
            },
            // One USD order, current month
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "USD-1",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 100m,
                CurrencyCode = "USD",
                ClientBasePrice = 100m,
                ClientChargePrice = 100m,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now
            },
            // Non-completed order should not appear in the rollup at all
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "USD-Pending",
                Description = "d",
                Status = OrderStatus.InProgress,
                Price = 999m,
                CurrencyCode = "USD",
                ClientBasePrice = 999m,
                ClientChargePrice = 999m,
                CreatedAt = now,
                UpdatedAt = now
            });
        await context.SaveChangesAsync();

        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var sut = new AnalyticsService(
            context,
            cache,
            new ReadModelCacheVersions(),
            Mock.Of<ILogger<AnalyticsService>>());

        var overview = await sut.GetOverviewAsync();

        Assert.True(overview.RevenueCurrencyMixed);
        Assert.Equal(2, overview.RevenueByCurrency.Count);

        // Sorted by TotalRevenue desc => USD(100) before GBP(30)
        Assert.Equal("USD", overview.RevenueByCurrency[0].CurrencyCode);
        Assert.Equal(100m, overview.RevenueByCurrency[0].TotalRevenue);
        Assert.Equal(100m, overview.RevenueByCurrency[0].MonthlyRevenue);
        Assert.Equal(1, overview.RevenueByCurrency[0].CompletedCount);
        Assert.Equal(100m, overview.RevenueByCurrency[0].AverageOrderValue);

        Assert.Equal("GBP", overview.RevenueByCurrency[1].CurrencyCode);
        Assert.Equal(30m, overview.RevenueByCurrency[1].TotalRevenue);
        Assert.Equal(20m, overview.RevenueByCurrency[1].MonthlyRevenue);
        Assert.Equal(2, overview.RevenueByCurrency[1].CompletedCount);
        Assert.Equal(15m, overview.RevenueByCurrency[1].AverageOrderValue);
    }

    [Fact]
    public async Task GetOverviewAsync_TreatsNullOrEmptyCurrencyAsUsd_InRollup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("AnalyticsOverview_NullCurrency_" + Guid.NewGuid())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var clientId = Guid.NewGuid();
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = clientId,
            UserId = Guid.NewGuid(),
            CompanyName = "Legacy Co",
            CreatedAt = DateTime.UtcNow
        });

        // CurrencyCode left as default ("USD" per entity default) and an explicit empty string
        // both must roll up under "USD" so legacy rows are not silently lost.
        context.LogoOrders.AddRange(
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "Legacy-1",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 5m,
                CurrencyCode = "",
                ClientBasePrice = 5m,
                ClientChargePrice = 5m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Title = "Legacy-2",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 7m,
                ClientBasePrice = 7m,
                ClientChargePrice = 7m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var sut = new AnalyticsService(
            context,
            cache,
            new ReadModelCacheVersions(),
            Mock.Of<ILogger<AnalyticsService>>());

        var overview = await sut.GetOverviewAsync();

        Assert.False(overview.RevenueCurrencyMixed);
        Assert.Equal("USD", overview.RevenueCurrencyCode);
        var rollup = Assert.Single(overview.RevenueByCurrency);
        Assert.Equal("USD", rollup.CurrencyCode);
        Assert.Equal(12m, rollup.TotalRevenue);
        Assert.Equal(2, rollup.CompletedCount);
    }
}
