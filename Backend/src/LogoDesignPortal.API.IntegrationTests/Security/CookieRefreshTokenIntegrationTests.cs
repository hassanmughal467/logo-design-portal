using System.Net;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class CookieRefreshTokenIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public CookieRefreshTokenIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RefreshToken_WithCookieSession_AndEmptyJsonBody_Returns200()
    {
        var client = CookieAuthHelper.CreateClient(_factory);
        var login = await CookieAuthHelper.LoginWithCookiesAsync(client);
        login.EnsureSuccessStatusCode();

        var cookieHeader = CookieAuthHelper.BuildCookieHeader(login);
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh-token")
        {
            Content = JsonContent.Create(new { }, options: IntegrationTestJson.Options)
        };
        CookieAuthHelper.ApplyCookieSession(refreshRequest, cookieHeader);

        var response = await client.SendAsync(refreshRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
