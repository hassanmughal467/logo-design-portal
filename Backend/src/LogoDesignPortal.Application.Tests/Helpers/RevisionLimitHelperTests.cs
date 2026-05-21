using LogoDesignPortal.Application.Helpers;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class RevisionLimitHelperTests
{
    [Theory]
    [InlineData(50, 2)]
    [InlineData(199.99, 2)]
    [InlineData(200, 4)]
    [InlineData(499, 4)]
    [InlineData(500, null)]
    [InlineData(1000, null)]
    public void GetRevisionLimitFromPrice_ReturnsExpected(decimal price, int? expected)
    {
        Assert.Equal(expected, RevisionLimitHelper.GetRevisionLimitFromPrice(price));
    }

    [Theory]
    [InlineData(0, 2, false, true)]
    [InlineData(1, 2, false, true)]
    [InlineData(2, 2, false, false)]
    [InlineData(2, 2, true, true)]
    [InlineData(10, null, false, true)]
    public void CanRequestRevision_RespectsLimitAndExtraFlag(
        int revisionCount,
        int? limit,
        bool allowExtra,
        bool expected)
    {
        Assert.Equal(expected, RevisionLimitHelper.CanRequestRevision(revisionCount, limit, allowExtra));
    }
}
