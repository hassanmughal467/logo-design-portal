using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class AuthControllerTests
{
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "client@test.com", password = "not-the-password" }, JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidSeededUser_Returns200WithToken()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "client@test.com", password = "Test@123" }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(body.TryGetProperty("token", out var tokenProp));
        Assert.False(string.IsNullOrWhiteSpace(tokenProp.GetString()));
    }

    [Fact]
    public async Task RefreshToken_InvalidBody_IsRejected()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh-token",
            new { refreshToken = "invalid-token" }, JsonOptions);
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest,
            $"Expected 401/400, got {response.StatusCode}");
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsGenericSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "unknown-user@example.com" }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ForgotPasswordBody>(JsonOptions);
        Assert.NotNull(body);
        Assert.Contains("If an account exists", body!.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(body.Email);
        Assert.Null(body.ResetToken);
        Assert.Null(body.ResetLink);
    }

    /// <summary>Uses shared AuthHelper contract for token shape (single place for test credentials).</summary>
    [Fact]
    public async Task Login_ViaAuthHelper_MatchesHappyPath()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    private sealed class ForgotPasswordBody
    {
        public string Message { get; set; } = string.Empty;
        public string? ResetToken { get; set; }
        public string? Email { get; set; }
        public string? ResetLink { get; set; }
    }
}
