using LogoDesignPortal.Application.Helpers;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class ClientCurrencyHelperTests
{
    [Theory]
    [InlineData("gbp", "GBP")]
    [InlineData("USD", "USD")]
    [InlineData(null, "USD")]
    [InlineData("XYZ", "USD")]
    public void NormalizeOrDefault_ReturnsExpected(string? input, string expected)
    {
        Assert.Equal(expected, ClientCurrencyHelper.NormalizeOrDefault(input));
    }

    [Fact]
    public void ResolveClientRevenueCurrency_PrefersSingleOrderCurrency_OverUnsetProfile()
    {
        var code = ClientCurrencyHelper.ResolveClientRevenueCurrency(null, new[] { "GBP" });
        Assert.Equal("GBP", code);
    }

    [Fact]
    public void ResolveClientRevenueCurrency_FallsBackToProfile_WhenNoOrders()
    {
        var code = ClientCurrencyHelper.ResolveClientRevenueCurrency("EUR", Array.Empty<string?>());
        Assert.Equal("EUR", code);
    }
}
