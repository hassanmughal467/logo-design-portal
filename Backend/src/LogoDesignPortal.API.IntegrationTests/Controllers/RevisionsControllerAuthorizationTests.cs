using System.Net;
using System.Net.Http.Headers;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class RevisionsControllerAuthorizationTests
{
    private readonly TestWebApplicationFactory _factory;

    public RevisionsControllerAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RequestRevision_AsDesigner_Returns403()
    {
        var orderId = await IntegrationDatabaseHelper.InsertPreviewDeliveredOrderAsync(
            _factory, _factory.ClientProfileId);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var form = MultipartTestHelper.CreateRevisionRequestForm();
        var response = await client.PostAsync($"/api/revisions/orders/{orderId}/request", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RequestRevision_WhenOrderNotPreviewDelivered_Returns400()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 50m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var form = MultipartTestHelper.CreateRevisionRequestForm();
        var response = await client.PostAsync($"/api/revisions/orders/{orderId}/request", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CanRequestRevision_AsClient_OnPreviewOrder_ReturnsTrue()
    {
        var orderId = await IntegrationDatabaseHelper.InsertPreviewDeliveredOrderAsync(
            _factory, _factory.ClientProfileId, price: 80m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/revisions/orders/{orderId}/can-request");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"canRequest\":true", json, StringComparison.OrdinalIgnoreCase);
    }
}
