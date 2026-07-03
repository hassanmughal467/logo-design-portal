using System.Security.Claims;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using LogoDesignPortal.API.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Infrastructure;

/// <summary>Unit-style tests for Hangfire dashboard authorization (no HTTP server).</summary>
public class HangfireAuthorizationFilterTests
{
    private static readonly JobStorage TestStorage = new MemoryStorage();

    private readonly HangfireAuthorizationFilter _filter = new();

    [Fact]
    public void Authorize_ReturnsFalse_WhenNotAuthenticated()
    {
        var context = CreateDashboardContext(authenticated: false, role: null);
        Assert.False(_filter.Authorize(context));
    }

    [Fact]
    public void Authorize_ReturnsTrue_ForSuperAdmin()
    {
        var context = CreateDashboardContext(authenticated: true, role: "SuperAdmin");
        Assert.True(_filter.Authorize(context));
    }

    [Fact]
    public void Authorize_ReturnsTrue_ForAdmin()
    {
        var context = CreateDashboardContext(authenticated: true, role: "Admin");
        Assert.True(_filter.Authorize(context));
    }

    [Fact]
    public void Authorize_ReturnsFalse_ForDesigner()
    {
        var context = CreateDashboardContext(authenticated: true, role: "Designer");
        Assert.False(_filter.Authorize(context));
    }

    [Fact]
    public void Authorize_ReturnsFalse_ForClient()
    {
        var context = CreateDashboardContext(authenticated: true, role: "Client");
        Assert.False(_filter.Authorize(context));
    }

    [Fact]
    public async Task DashboardAccessMiddleware_Returns403_ForAuthenticatedDesigner()
    {
        var middleware = new HangfireDashboardAccessMiddleware(_ => Task.CompletedTask);
        var httpContext = CreateHttpContext(authenticated: true, role: "Designer");

        await middleware.InvokeAsync(httpContext);

        Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task DashboardAccessMiddleware_AllowsAdminThrough()
    {
        var nextCalled = false;
        var middleware = new HangfireDashboardAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var httpContext = CreateHttpContext(authenticated: true, role: "Admin");

        await middleware.InvokeAsync(httpContext);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    private static DashboardContext CreateDashboardContext(bool authenticated, string? role)
    {
        var httpContext = CreateHttpContext(authenticated, role);
        return new AspNetCoreDashboardContext(TestStorage, new DashboardOptions(), httpContext);
    }

    private static HttpContext CreateHttpContext(bool authenticated, string? role)
    {
        var claims = new List<Claim>();
        if (authenticated)
        {
            claims.Add(new Claim(ClaimTypes.Name, "test@example.com"));
            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = authenticated
            ? new ClaimsIdentity(claims, "TestAuth")
            : new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<HangfireAuthorizationFilter>>(NullLogger<HangfireAuthorizationFilter>.Instance);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            RequestServices = serviceProvider
        };
        httpContext.Request.Path = "/hangfire";
        return httpContext;
    }
}
