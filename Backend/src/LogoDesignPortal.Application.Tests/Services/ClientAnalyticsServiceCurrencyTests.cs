using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Verifies that <see cref="ClientAnalyticsService"/> propagates each client's
/// <c>ClientProfile.CurrencyCode</c> onto every revenue-bearing DTO so the
/// frontend never has to fall back to a hard-coded "USD" symbol.
/// </summary>
public class ClientAnalyticsServiceCurrencyTests
{
    private static ApplicationDbContext CreateContext(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name + Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClientProfile SeedClient(ApplicationDbContext context, string company, string currencyCode)
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Email = $"{Guid.NewGuid():N}@example.com",
            FirstName = "Test",
            LastName = "Client",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        });
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = company,
            CurrencyCode = currencyCode,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };
        context.ClientProfiles.Add(client);
        return client;
    }

    private static LogoOrder SeedCompletedOrder(Guid clientId, decimal price, string currencyCode, int daysAgo = 5)
    {
        return new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Title = "T",
            Description = "d",
            Status = OrderStatus.Completed,
            Price = price,
            CurrencyCode = currencyCode,
            ClientBasePrice = price,
            ClientChargePrice = price,
            CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
            UpdatedAt = DateTime.UtcNow.AddDays(-daysAgo)
        };
    }

    [Fact]
    public async Task GetOverviewAsync_UsesOrderGbp_WhenProfileStillUsd()
    {
        await using var context = CreateContext("ClientOverview_OrderGbp_");
        var client = SeedClient(context, "kevin walker", "USD");
        context.LogoOrders.Add(SeedCompletedOrder(client.Id, 13m, "GBP"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var overview = await sut.GetOverviewAsync();
        var top = await sut.GetTopClientsAsync(10);

        Assert.Equal("GBP", overview.TopClientCurrencyCode);
        Assert.Equal("GBP", top.Items.Single().CurrencyCode);
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsTopClientCurrency_WhenTopClientUsesGbp()
    {
        await using var context = CreateContext("ClientOverview_GBP_");
        var gbpClient = SeedClient(context, "Kelvin Walter", "GBP");
        context.LogoOrders.Add(SeedCompletedOrder(gbpClient.Id, 13m, "GBP"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var overview = await sut.GetOverviewAsync();

        Assert.Equal(13m, overview.TopClientRevenue);
        Assert.Equal("GBP", overview.TopClientCurrencyCode);
    }

    [Fact]
    public async Task GetTopClientsAsync_AssignsEachClientsCurrency()
    {
        await using var context = CreateContext("TopClients_Currency_");
        var gbpClient = SeedClient(context, "GBP Co", "GBP");
        var usdClient = SeedClient(context, "USD Co", "USD");
        context.LogoOrders.AddRange(
            SeedCompletedOrder(gbpClient.Id, 100m, "GBP"),
            SeedCompletedOrder(usdClient.Id, 50m, "USD"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var top = await sut.GetTopClientsAsync(10);

        Assert.Equal(2, top.Items.Count);
        Assert.Equal("GBP", top.Items.Single(i => i.ClientId == gbpClient.Id).CurrencyCode);
        Assert.Equal("USD", top.Items.Single(i => i.ClientId == usdClient.Id).CurrencyCode);
    }

    [Fact]
    public async Task GetClientLifetimeValueAsync_AssignsEachClientsCurrency()
    {
        await using var context = CreateContext("Lifetime_Currency_");
        var gbpClient = SeedClient(context, "GBP Ltd", "GBP");
        var eurClient = SeedClient(context, "EUR SARL", "EUR");
        context.LogoOrders.AddRange(
            SeedCompletedOrder(gbpClient.Id, 200m, "GBP"),
            SeedCompletedOrder(eurClient.Id, 75m, "EUR"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var ltv = await sut.GetClientLifetimeValueAsync();

        Assert.Equal("GBP", ltv.Items.Single(i => i.ClientId == gbpClient.Id).CurrencyCode);
        Assert.Equal("EUR", ltv.Items.Single(i => i.ClientId == eurClient.Id).CurrencyCode);
    }

    [Fact]
    public async Task GetClientMonthlyRevenueAsync_UsesClientsCurrency()
    {
        await using var context = CreateContext("Monthly_Currency_");
        var gbpClient = SeedClient(context, "Kelvin Walter", "GBP");
        context.LogoOrders.Add(SeedCompletedOrder(gbpClient.Id, 13m, "GBP"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var monthly = await sut.GetClientMonthlyRevenueAsync(gbpClient.Id, 12);

        Assert.Equal("GBP", monthly.CurrencyCode);
    }

    [Fact]
    public async Task GetClientRevenueTrendAsync_FlagsMixedCurrencies_WhenWindowHasMultipleCurrencies()
    {
        await using var context = CreateContext("Trend_Mixed_");
        var gbpClient = SeedClient(context, "GBP Co", "GBP");
        var usdClient = SeedClient(context, "USD Co", "USD");
        context.LogoOrders.AddRange(
            SeedCompletedOrder(gbpClient.Id, 13m, "GBP"),
            SeedCompletedOrder(usdClient.Id, 50m, "USD"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var trend = await sut.GetClientRevenueTrendAsync(6);

        Assert.True(trend.CurrencyMixed);
        Assert.Equal("USD", trend.CurrencyCode);
    }

    [Fact]
    public async Task GetClientRevenueTrendAsync_ReturnsSingleCurrency_WhenAllOrdersShareCurrency()
    {
        await using var context = CreateContext("Trend_Single_");
        var gbpClient = SeedClient(context, "GBP Co", "GBP");
        context.LogoOrders.AddRange(
            SeedCompletedOrder(gbpClient.Id, 5m, "GBP"),
            SeedCompletedOrder(gbpClient.Id, 8m, "GBP"));
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var trend = await sut.GetClientRevenueTrendAsync(6);

        Assert.False(trend.CurrencyMixed);
        Assert.Equal("GBP", trend.CurrencyCode);
    }

    [Fact]
    public async Task GetClientGrowthMetricsAsync_AssignsEachClientsCurrency()
    {
        await using var context = CreateContext("Growth_Currency_");
        var gbpClient = SeedClient(context, "GBP Ltd", "GBP");
        var now = DateTime.UtcNow;
        var lastMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1).AddDays(5);
        context.LogoOrders.Add(new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = gbpClient.Id,
            Title = "T",
            Description = "d",
            Status = OrderStatus.Completed,
            Price = 50m,
            CurrencyCode = "GBP",
            ClientBasePrice = 50m,
            ClientChargePrice = 50m,
            CreatedAt = lastMonth,
            UpdatedAt = lastMonth
        });
        await context.SaveChangesAsync();

        var sut = new ClientAnalyticsService(context);

        var growth = await sut.GetClientGrowthMetricsAsync();

        Assert.Equal("GBP", growth.Items.Single(i => i.ClientId == gbpClient.Id).CurrencyCode);
    }
}
