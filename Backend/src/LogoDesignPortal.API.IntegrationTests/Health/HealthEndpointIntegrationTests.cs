using System.Net;
using System.Text.Json;
using LogoDesignPortal.API.IntegrationTests;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Health;

[Collection("Integration")]
public class HealthEndpointIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public HealthEndpointIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task HealthLive_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("status", out _));
    }

    [Fact]
    public async Task HealthReady_IncludesDatabaseCheck()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("database_schema", json, StringComparison.OrdinalIgnoreCase);
    }
}
