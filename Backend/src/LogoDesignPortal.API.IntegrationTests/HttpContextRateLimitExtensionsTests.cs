using LogoDesignPortal.API.Extensions;
using Microsoft.AspNetCore.Http;
using System.Net;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests;

public class HttpContextRateLimitExtensionsTests
{
    [Fact]
    public void GetRateLimitClientId_IgnoresClientSuppliedForwardedForHeader()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");
        context.Request.Headers["X-Forwarded-For"] = "9.9.9.9";

        var clientId = context.GetRateLimitClientId();

        Assert.Equal("203.0.113.10", clientId);
    }

    [Fact]
    public void GetRateLimitClientId_DifferentSpoofedHeadersStillResolveToSameRealIp()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");
        context.Request.Headers["X-Forwarded-For"] = "1.1.1.1";
        var first = context.GetRateLimitClientId();

        context.Request.Headers["X-Forwarded-For"] = "2.2.2.2";
        var second = context.GetRateLimitClientId();

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetRateLimitClientId_NoRemoteIp_ReturnsUnknown()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        var clientId = context.GetRateLimitClientId();

        Assert.Equal("unknown", clientId);
    }
}
