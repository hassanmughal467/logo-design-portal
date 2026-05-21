using LogoDesignPortal.Application.Caching;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Caching;

public class ReadModelCacheVersionsTests
{
    [Fact]
    public void BumpOrders_IncrementsOrdersEpoch()
    {
        var versions = new ReadModelCacheVersions();
        var before = versions.OrdersEpoch;
        versions.BumpOrders();
        Assert.True(versions.OrdersEpoch > before);
    }

    [Fact]
    public void BumpAnalytics_IsIndependentFromOrders()
    {
        var versions = new ReadModelCacheVersions();
        var ordersBefore = versions.OrdersEpoch;
        versions.BumpAnalytics();
        Assert.Equal(ordersBefore, versions.OrdersEpoch);
        Assert.True(versions.AnalyticsEpoch >= 1);
    }
}
