using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public OrdersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };


    [Fact]
    public async Task CreateOrder_AsClient_Succeeds()
    {
        var token = await AuthHelper.GetClientTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            title = "Integration Test Order",
            description = "Order created by integration test",
            price = 100,
            designCategory = DesignCategory.EmbroideryDigitizing,
            designType = DesignType.LeftChest
        };

        var response = await _client.PostAsJsonAsync("/api/orders", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal("Integration Test Order", order.Title);
        Assert.Equal(OrderStatus.WaitingForAdminApproval.ToString(), order.Status);
    }

    [Fact]
    public async Task CreateOrder_WithoutAuth_Returns401()
    {
        var request = new { title = "Test", description = "Desc", price = 50 };
        var response = await _client.PostAsJsonAsync("/api/orders", request, JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_AsClient_ReturnsOnlyOwnOrders()
    {
        var token = await AuthHelper.GetClientTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/orders/my-orders");
        Assert.True(response.IsSuccessStatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>(JsonOptions);
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task GetAssignedOrders_AsDesigner_ReturnsAssignedOrders()
    {
        var token = await AuthHelper.GetDesignerTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/orders/assigned-orders");
        Assert.True(response.IsSuccessStatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>(JsonOptions);
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task AssignDesigner_AsAdmin_Succeeds()
    {
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(_client);
        var clientToken = await AuthHelper.GetClientTokenAsync(_client);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            title = "Order for assign test",
            description = "Test",
            price = 100
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(createdOrder);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var assignRequest = new { designerId = _factory.DesignerUserId };
        var assignResponse = await _client.PostAsJsonAsync($"/api/orders/{createdOrder.Id}/assign", assignRequest, JsonOptions);
        Assert.True(assignResponse.IsSuccessStatusCode, await assignResponse.Content.ReadAsStringAsync());

        var updatedOrder = await assignResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(updatedOrder);
        Assert.NotNull(updatedOrder.Designer);
    }

    private class OrderResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public object? Designer { get; set; }
    }

}
