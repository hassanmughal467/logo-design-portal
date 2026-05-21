using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Security;

[Collection("Integration")]
public class IdorAndCrossTenantIntegrationTests
{
    private readonly TestWebApplicationFactory _factory;

    public IdorAndCrossTenantIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetInvoiceById_UnknownGuid_AsClient_Returns404Or403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var get = await client.GetAsync($"/api/invoices/{Guid.NewGuid()}");
        Assert.True(
            get.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"IDOR probe must not return invoice data. Got {get.StatusCode}");
    }

    [Fact]
    public async Task FileDownload_PathTraversalInFileId_Returns400Or404()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/files/..%2F..%2Fetc%2Fpasswd/download");
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest
                or HttpStatusCode.NotFound
                or HttpStatusCode.Unauthorized,
            $"Path traversal attempt must not succeed. Got {response.StatusCode}");
    }

}
