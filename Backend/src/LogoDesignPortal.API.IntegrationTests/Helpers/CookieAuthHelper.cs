using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogoDesignPortal.API.IntegrationTests.Helpers;

/// <summary>
/// Cookie-session auth helpers for CSRF and cookie-auth integration tests.
/// </summary>
public static class CookieAuthHelper
{
    public const string CsrfHeaderName = "X-XSRF-TOKEN";
    public const string CsrfCookieName = "ldp_csrf";
    public const string AccessCookieName = "ldp_access";

    /// <summary>Cookie jar disabled — session cookies are applied explicitly so CSRF header matches Set-Cookie values.</summary>
    public static HttpClient CreateClient(TestWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

    public static async Task<HttpResponseMessage> LoginWithCookiesAsync(
        HttpClient client,
        string email = "client@test.com",
        string password = "Test@123")
    {
        return await client.PostAsJsonAsync("/api/auth/login", new { email, password }, IntegrationTestJson.Options);
    }

    public static string? ExtractSetCookieValue(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
            return null;

        foreach (var header in headers)
        {
            if (header.StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase))
            {
                var valuePart = header.Split(';')[0];
                return valuePart[(cookieName.Length + 1)..];
            }
        }

        return null;
    }

    public static void SetCsrfHeader(HttpRequestMessage request, string csrfToken) =>
        request.Headers.TryAddWithoutValidation(CsrfHeaderName, csrfToken);

    public static string BuildCookieHeader(HttpResponseMessage loginResponse)
    {
        if (!loginResponse.Headers.TryGetValues("Set-Cookie", out var headers))
            throw new InvalidOperationException("Login response did not set cookies.");

        var parts = new List<string>();
        foreach (var header in headers)
        {
            var segment = header.Split(';')[0];
            parts.Add(segment);
        }

        return string.Join("; ", parts);
    }

    public static async Task<(HttpClient Client, string CsrfToken, string CookieHeader)> LoginCookieSessionAsync(
        TestWebApplicationFactory factory,
        string email = "client@test.com",
        string password = "Test@123")
    {
        var client = CreateClient(factory);
        var login = await LoginWithCookiesAsync(client, email, password);
        if (!login.IsSuccessStatusCode)
        {
            var body = await login.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Cookie login failed ({login.StatusCode}): {body}");
        }

        var csrf = ExtractSetCookieValue(login, CsrfCookieName)
                   ?? throw new InvalidOperationException("Login response did not set CSRF cookie.");
        var cookieHeader = BuildCookieHeader(login);
        return (client, csrf, cookieHeader);
    }

    public static void ApplyCookieSession(HttpRequestMessage request, string cookieHeader, string? csrfToken = null)
    {
        request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        if (csrfToken != null)
            SetCsrfHeader(request, csrfToken);
    }
}
