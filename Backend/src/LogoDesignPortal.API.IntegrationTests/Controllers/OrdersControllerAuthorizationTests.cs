using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

/// <summary>
/// Authorization and permission enforcement for order endpoints.
/// </summary>
[Collection("Integration")]
public class OrdersControllerAuthorizationTests
{
    private readonly TestWebApplicationFactory _factory;

    public OrdersControllerAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateOrder_WithoutBearer_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders",
            new { title = "x", description = "y", price = 1 }, IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_AsDesigner_OnUnassignedOrder_Returns400Or403()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Authz test", description = "d", price = 50 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(IntegrationTestJson.Options);
        Assert.NotNull(order);

        var designer = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(designer);
        designer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", designerToken);

        var response = await designer.PutAsJsonAsync($"/api/orders/{order!.Id}/status",
            new { status = "InProgress", notes = "not assigned" }, IntegrationTestJson.Options);

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Forbidden,
            $"Expected 400/403, got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task GetOrderById_AsOtherClient_Returns403Or404()
    {
        var clientA = _factory.CreateClient();
        var tokenA = await AuthHelper.GetClientTokenAsync(clientA);
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var create = await clientA.PostAsJsonAsync("/api/orders",
            new { title = "Private", description = "d", price = 10 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(IntegrationTestJson.Options);

        // Second client cannot exist in seed — use designer token as non-owner
        var other = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(other);
        other.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", designerToken);

        var get = await other.GetAsync($"/api/orders/{order!.Id}");
        Assert.True(
            get.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.BadRequest,
            $"Unexpected {get.StatusCode}");
    }

    [Fact]
    public async Task SendPreviewBatch_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Batch", description = "d", price = 10 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(IntegrationTestJson.Options);

        var response = await client.PostAsJsonAsync($"/api/orders/{order!.Id}/send-preview-batch",
            new { previewBatchId = Guid.NewGuid() }, IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed class OrderIdResponse
    {
        public Guid Id { get; set; }
    }
}
