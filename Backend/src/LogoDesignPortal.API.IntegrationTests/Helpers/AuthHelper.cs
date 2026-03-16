using System.Net.Http.Json;
using System.Text.Json;

namespace LogoDesignPortal.API.IntegrationTests.Helpers;

/// <summary>
/// Helper for obtaining JWT tokens for integration tests.
/// </summary>
public static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Login and return the access token. Uses test user credentials from seeded data.
    /// </summary>
    public static async Task<string> GetAccessTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password }, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed ({response.StatusCode}): {body}");
        }

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return content?.Token ?? throw new InvalidOperationException("Login response missing access token");
    }

    /// <summary>
    /// Gets access token for Client role (client@test.com / Test@123).
    /// </summary>
    public static Task<string> GetClientTokenAsync(HttpClient client) =>
        GetAccessTokenAsync(client, "client@test.com", "Test@123");

    /// <summary>
    /// Gets access token for Admin role (admin@test.com / Test@123).
    /// </summary>
    public static Task<string> GetAdminTokenAsync(HttpClient client) =>
        GetAccessTokenAsync(client, "admin@test.com", "Test@123");

    /// <summary>
    /// Gets access token for Designer role (designer@test.com / Test@123).
    /// </summary>
    public static Task<string> GetDesignerTokenAsync(HttpClient client) =>
        GetAccessTokenAsync(client, "designer@test.com", "Test@123");

    /// <summary>
    /// Gets access token for SuperAdmin role (superadmin@test.com / Test@123).
    /// Note: Seed uses superadmin@test.com; production may use superadmin@logodesign.com.
    /// </summary>
    public static Task<string> GetSuperAdminTokenAsync(HttpClient client) =>
        GetAccessTokenAsync(client, "superadmin@test.com", "Test@123");

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
