using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class DesignerPayoutControllerAuthorizationTests
{
    private readonly TestWebApplicationFactory _factory;

    public DesignerPayoutControllerAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPayoutSummary_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/designer-invoice/payout-summary");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPayoutSummary_AsAdmin_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/designer-invoice/payout-summary");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDesignerEligibleOrders_AsDesigner_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/designer-payout/designers/{Guid.NewGuid()}/eligible-orders");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyEligibleOrders_AsDesigner_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/designer-payout/me/eligible-orders");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GenerateInvoice_AsDesigner_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/designer-payout/generate-invoice",
            new { designerId = Guid.NewGuid(), orderIds = Array.Empty<Guid>(), items = Array.Empty<object>() },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ProposeDesignerPrice_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/designer-payout/pricing/propose",
            new { orderId = Guid.NewGuid(), proposedPrice = 100m },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
