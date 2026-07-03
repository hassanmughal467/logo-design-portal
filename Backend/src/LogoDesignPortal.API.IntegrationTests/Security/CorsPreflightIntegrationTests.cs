using System.Net;
using LogoDesignPortal.API.IntegrationTests;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class CorsPreflightIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public CorsPreflightIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Options_Login_FromProductionAdminOrigin_ReturnsCorsHeaders()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.TryAddWithoutValidation("Origin", "https://admin.hawkmerchandising.com");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "POST");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "content-type,x-xsrf-token");

        var response = await client.SendAsync(request);

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Expected 200/204, got {response.StatusCode}");
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("https://admin.hawkmerchandising.com", origins!);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Credentials", out var creds));
        Assert.Contains("true", creds!, StringComparer.OrdinalIgnoreCase);
    }
}
