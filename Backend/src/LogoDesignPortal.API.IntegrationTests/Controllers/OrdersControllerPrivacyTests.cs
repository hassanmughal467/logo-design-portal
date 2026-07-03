using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

/// <summary>
/// Role masking: clients must not receive designer PII in order DTOs.
/// </summary>
[Collection("Integration")]
public class OrdersControllerPrivacyTests
{
    private readonly TestWebApplicationFactory _factory;

    public OrdersControllerPrivacyTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetOrderById_AsClient_MasksDesignerIdentity()
    {
        var admin = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(admin);

        var clientHttp = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(clientHttp);
        clientHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var create = await clientHttp.PostAsJsonAsync("/api/orders",
            new { title = "Mask test", description = "d", price = 99 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<OrderJson>(IntegrationTestJson.Options);
        Assert.NotNull(created);

        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await admin.PostAsJsonAsync($"/api/orders/{created!.Id}/assign",
            new { designerId = _factory.DesignerUserId }, IntegrationTestJson.Options);

        var get = await clientHttp.GetAsync($"/api/orders/{created.Id}");
        get.EnsureSuccessStatusCode();
        var json = await get.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("designer", out var designer) && designer.ValueKind != JsonValueKind.Null)
        {
            Assert.Fail("Client order response must not include designer object.");
        }

        if (root.TryGetProperty("assignedDesignerDisplayName", out var display))
        {
            Assert.Equal("Company Design Team", display.GetString());
        }
    }

    [Fact]
    public async Task GetMyOrders_AsClient_DoesNotExposeDesignerObject()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/orders/my-orders");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var order in doc.RootElement.EnumerateArray())
        {
            if (order.TryGetProperty("designer", out var designer) && designer.ValueKind == JsonValueKind.Object)
            {
                Assert.Fail("Client order list must not include designer object.");
            }
        }
    }

    private sealed class OrderJson
    {
        public Guid Id { get; set; }
    }
}
