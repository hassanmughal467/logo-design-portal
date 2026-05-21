using System.Net;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class RefreshTokenAndReplayIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public RefreshTokenAndReplayIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RefreshToken_WithGarbageToken_Returns401Or400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken = "not-a-valid-refresh-token" },
            IntegrationTestJson.Options);
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized,
            $"Expected 400/401, got {response.StatusCode}");
    }

    [Fact]
    public async Task Login_TwiceSameCredentials_ReturnsDistinctTokens()
    {
        var client = _factory.CreateClient();
        var body = new { email = "client@test.com", password = "Test@123" };
        var first = await client.PostAsJsonAsync("/api/auth/login", body, IntegrationTestJson.Options);
        var second = await client.PostAsJsonAsync("/api/auth/login", body, IntegrationTestJson.Options);
        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();
        var a = await first.Content.ReadFromJsonAsync<LoginTokens>(IntegrationTestJson.Options);
        var b = await second.Content.ReadFromJsonAsync<LoginTokens>(IntegrationTestJson.Options);
        Assert.NotEqual(a!.Token, b!.Token);
    }

    private sealed class LoginTokens
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
