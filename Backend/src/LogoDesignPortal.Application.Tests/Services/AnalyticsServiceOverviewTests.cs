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
    }
}
