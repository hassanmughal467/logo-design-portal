using LogoDesignPortal.Application.Services;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Services;

/// <summary>
/// Verifies that <see cref="ClientChurnAnalyticsService"/> tags each
/// client-revenue DTO with the correct ISO currency so the Churn Prediction
/// tab never falls back to a hardcoded USD symbol.
/// </summary>
public class ClientChurnAnalyticsServiceCurrencyTests
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
            CreatedAt = DateTime.UtcNow.AddDays(-200)
        });
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = company,
            CurrencyCode = currencyCode,
            CreatedAt = DateTime.UtcNow.AddDays(-180)
        };
        context.ClientProfiles.Add(client);
        return client;
    }

    [Fact]
    public async Task GetClientRiskScoresAsync_TagsEachItemWithClientCurrency()
    {
        await using var context = CreateContext("Risk_Currency_");
        var gbpClient = SeedClient(context, "GBP Co", "GBP");
        var usdClient = SeedClient(context, "USD Co", "USD");

        var ordersAt = DateTime.UtcNow.AddDays(-10);
        context.LogoOrders.AddRange(
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = gbpClient.Id,
                Title = "T",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 200m,
                CurrencyCode = "GBP",
                ClientBasePrice = 200m,
                ClientChargePrice = 200m,
                CreatedAt = ordersAt,
                UpdatedAt = ordersAt
            },
            new LogoOrder
            {
                Id = Guid.NewGuid(),
                ClientId = usdClient.Id,
                Title = "T",
                Description = "d",
                Status = OrderStatus.Completed,
                Price = 50m,
                CurrencyCode = "USD",
                ClientBasePrice = 50m,
                ClientChargePrice = 50m,
                CreatedAt = ordersAt,
                UpdatedAt = ordersAt
            });
        await context.SaveChangesAsync();

        var sut = new ClientChurnAnalyticsService(context);

        var risks = await sut.GetClientRiskScoresAsync();

        Assert.Equal("GBP", risks.Items.Single(i => i.ClientId == gbpClient.Id).CurrencyCode);
        Assert.Equal("USD", risks.Items.Single(i => i.ClientId == usdClient.Id).CurrencyCode);
    }

    [Fact]
    public async Task GetClientChurnAlertsAsync_TagsAlertWithClientCurrency_AndIncludesItInMessage()
    {
        await using var context = CreateContext("ChurnAlerts_Currency_");
        var gbpClient = SeedClient(context, "Kelvin Walter", "GBP");

        var inactiveAt = DateTime.UtcNow.AddDays(-60);
        context.LogoOrders.Add(new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = gbpClient.Id,
            Title = "T",
            Description = "d",
            Status = OrderStatus.Completed,
            Price = 13m,
            CurrencyCode = "GBP",
            ClientBasePrice = 13m,
            ClientChargePrice = 13m,
            CreatedAt = inactiveAt,
            UpdatedAt = inactiveAt
        });
        await context.SaveChangesAsync();

        var sut = new ClientChurnAnalyticsService(context);

        var alerts = await sut.GetClientChurnAlertsAsync(30);

        var alert = Assert.Single(alerts.Items);
        Assert.Equal("GBP", alert.CurrencyCode);
        Assert.Contains("GBP", alert.AlertMessage);
        Assert.DoesNotContain("$", alert.AlertMessage);
    }
}
