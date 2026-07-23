using System.Net;
using System.Text.Json;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class HealthEndpointsTests
{
    private readonly TestWebApplicationFactory _factory;

    public HealthEndpointsTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Live_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Live_ExecutesNoRegisteredChecks()
    {
        // Predicate = _ => false must exclude every registered check (database_schema, hangfire, redis).
        // Uses the same JSON writer as /health/ready so an empty "results" array is a genuine proof
        // that zero checks ran, not just an artifact of the default plaintext writer.
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Equal(0, root.GetProperty("results").GetArrayLength());
    }

    [Fact]
    public async Task Ready_StillExecutesRegisteredChecks()
    {
        // Regression guard: adding /health/live must not alter /health/ready's existing behavior.
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("database_schema", body);
        Assert.Contains("hangfire", body);
    }
}
