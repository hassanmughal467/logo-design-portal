using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class CsrfCookieAuthIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public CsrfCookieAuthIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task MutatingRequest_WithCookieSession_WithoutCsrfHeader_Returns403()
    {
        var (client, _, cookieHeader) = await CookieAuthHelper.LoginCookieSessionAsync(_factory);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        CookieAuthHelper.ApplyCookieSession(request, cookieHeader);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CSRF", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MutatingRequest_WithCookieSession_AndMatchingCsrfHeader_Succeeds()
    {
        var (client, csrf, cookieHeader) = await CookieAuthHelper.LoginCookieSessionAsync(_factory);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        CookieAuthHelper.ApplyCookieSession(request, cookieHeader, csrf);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MutatingRequest_WithCookieSession_MismatchedCsrfHeader_Returns403()
    {
        var (client, _, cookieHeader) = await CookieAuthHelper.LoginCookieSessionAsync(_factory);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        CookieAuthHelper.ApplyCookieSession(request, cookieHeader, "not-the-cookie-value");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SafeGet_WithCookieSession_WithoutCsrfHeader_Returns200()
    {
        var (client, _, cookieHeader) = await CookieAuthHelper.LoginCookieSessionAsync(_factory);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders/my-orders");
        CookieAuthHelper.ApplyCookieSession(request, cookieHeader);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MutatingRequest_WithBearerAuth_SkipsCsrfValidation()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCookieResponse_SetsAccessAndCsrfCookies()
    {
        var client = CookieAuthHelper.CreateClient(_factory);
        var login = await CookieAuthHelper.LoginWithCookiesAsync(client);

        login.EnsureSuccessStatusCode();
        Assert.NotNull(CookieAuthHelper.ExtractSetCookieValue(login, CookieAuthHelper.AccessCookieName));
        Assert.NotNull(CookieAuthHelper.ExtractSetCookieValue(login, CookieAuthHelper.CsrfCookieName));
    }
}
