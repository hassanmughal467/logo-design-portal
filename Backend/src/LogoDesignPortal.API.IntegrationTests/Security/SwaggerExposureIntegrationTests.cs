using System.Net;
using LogoDesignPortal.API.IntegrationTests;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class SwaggerExposureIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public SwaggerExposureIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Swagger_NotAvailable_InTestingEnvironment()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_NotAvailable_InTestingEnvironment()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
