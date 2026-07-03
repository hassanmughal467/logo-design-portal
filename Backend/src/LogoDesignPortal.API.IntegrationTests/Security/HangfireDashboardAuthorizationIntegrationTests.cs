using System.Net;
using System.Net.Http.Headers;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class HangfireDashboardAuthorizationIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public HangfireDashboardAuthorizationIntegrationTests(TestWebApplicationFactory factory) =>
        _factory = factory;

    [Fact]
    public async Task HangfireDashboard_WithoutAuth_Returns401OrRedirect()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/hangfire");

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized
            || response.StatusCode == HttpStatusCode.Redirect
            || response.StatusCode == HttpStatusCode.Found,
            $"Expected 401 or redirect, got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Fact]
    public async Task HangfireDashboard_AsDesigner_Returns403()
    {
        var client = await CreateAuthenticatedClientAsync(AuthHelper.GetDesignerTokenAsync);
        var response = await client.GetAsync("/hangfire");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HangfireDashboard_AsAdmin_Returns200()
    {
        var client = await CreateAuthenticatedClientAsync(AuthHelper.GetAdminTokenAsync);
        var response = await client.GetAsync("/hangfire");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HangfireDashboard_AsSuperAdmin_Returns200()
    {
        var client = await CreateAuthenticatedClientAsync(AuthHelper.GetSuperAdminTokenAsync);
        var response = await client.GetAsync("/hangfire");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(Func<HttpClient, Task<string>> getTokenAsync)
    {
        var client = _factory.CreateClient();
        var token = await getTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
