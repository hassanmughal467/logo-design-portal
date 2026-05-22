using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class OrdersControllerTests
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
    public async Task CreateOrder_AsClient_AdminAndSuperAdminReceiveNewOrderNotification()
    {
        var clientHttp = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(clientHttp);
        clientHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var createResponse = await clientHttp.PostAsJsonAsync("/api/orders", new
        {
            title = "Notify Admin Test Order",
            description = "Integration test for admin notifications",
            price = 99,
            designCategory = DesignCategory.EmbroideryDigitizing,
            designType = DesignType.LeftChest
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var adminHttp = _factory.CreateClient();
        var adminToken = await AuthHelper.GetAdminTokenAsync(adminHttp);
        adminHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var adminNotifications = await adminHttp.GetFromJsonAsync<List<NotificationItem>>("/api/notifications", JsonOptions);
        Assert.NotNull(adminNotifications);
        Assert.Contains(adminNotifications!, n =>
            n.Title == "New Order Submitted"
            && (n.Message?.Contains("placed a new order", StringComparison.OrdinalIgnoreCase) ?? false)
            && (n.OrderId == created.Id || n.ReferenceId == created.Id));

        var superAdminHttp = _factory.CreateClient();
        var superAdminToken = await AuthHelper.GetSuperAdminTokenAsync(superAdminHttp);
        superAdminHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", superAdminToken);

        var superAdminNotifications = await superAdminHttp.GetFromJsonAsync<List<NotificationItem>>("/api/notifications", JsonOptions);
        Assert.NotNull(superAdminNotifications);
        Assert.Contains(superAdminNotifications!, n =>
            n.Title == "New Order Submitted"
            && (n.Message?.Contains("placed a new order", StringComparison.OrdinalIgnoreCase) ?? false)
            && (n.OrderId == created.Id || n.ReferenceId == created.Id));
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
    public async Task GetOrderById_AsOwningClient_Returns200()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            title = "Owned order",
            description = "Detail test",
            price = 75
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidTransition_Returns400()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            title = "Status machine test",
            description = "x",
            price = 50
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(created);

        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var badTransition = await client.PutAsJsonAsync($"/api/orders/{created.Id}/status",
            new { status = "Completed", notes = "invalid jump" }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, badTransition.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_WithoutViewAllOrders_Returns403()
    {
        var client = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", designerToken);

        var response = await client.GetAsync("/api/orders?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    [Fact]
    public async Task AssignDesigner_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientToken);

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            title = "Forbidden assign",
            description = "Client should not assign designers.",
            price = 100
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        Assert.NotNull(createdOrder);

        var assignResponse = await client.PostAsJsonAsync($"/api/orders/{createdOrder.Id}/assign",
            new { designerId = _factory.DesignerUserId }, JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, assignResponse.StatusCode);
    }

    [Fact]
    public async Task CreateManualCompletedOrder_AsAdmin_WithBackdateAndFiles_Succeeds()
    {
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var backdatedUtc = DateTime.UtcNow.AddDays(-10);
        var payload = new
        {
            clientUserId = _factory.ClientUserId,
            designerUserId = _factory.DesignerUserId,
            title = "Manual Completed Integration Order",
            description = "Completed externally",
            price = 245.5m,
            completedAt = backdatedUtc,
            notes = "Backdated quick entry"
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(payload, JsonOptions)), "order");

        var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "files", "final-proof.pdf");

        var response = await _client.PostAsync("/api/orders/manual-completed", form);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Expected Created but got {(int)response.StatusCode} ({response.StatusCode}). Body: {body}");

        var created = JsonSerializer.Deserialize<OrderResponse>(body, JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(OrderStatus.Completed.ToString(), created.Status);
        Assert.Equal(OrderSource.ManualCompleted.ToString(), created.OrderSource);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await db.LogoOrders.FindAsync(created.Id);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Completed, order!.Status);
        Assert.Equal(OrderSource.ManualCompleted, order.OrderSource);
        Assert.True(order.BillingEligible);
        Assert.True(order.CompletedDate.HasValue);
        Assert.Equal(backdatedUtc.Date, order.CompletedDate!.Value.Date);
        Assert.Equal(backdatedUtc.Date, order.CreatedAt.Date);

        var files = db.LogoFiles.Where(f => f.OrderId == created.Id).ToList();
        Assert.NotEmpty(files);
        Assert.All(files, f =>
        {
            Assert.Equal(FileType.Final, f.FileType);
            Assert.True(f.IsAdminApproved);
            Assert.True(f.IsVisibleToClient);
        });
    }

    [Fact]
    public async Task CreateManualCompletedOrder_AsAdmin_WithoutFiles_Returns400()
    {
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var payload = new
        {
            clientUserId = _factory.ClientUserId,
            designerUserId = _factory.DesignerUserId,
            title = "Manual Completed Missing Files",
            description = "Should fail when files are missing",
            price = 125m,
            completedAt = DateTime.UtcNow.AddDays(-2),
            notes = "No files attached"
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(payload, JsonOptions)), "order");

        var response = await _client.PostAsync("/api/orders/manual-completed", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("At least one final file is required", body, StringComparison.OrdinalIgnoreCase);
    }

    private class OrderResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
        public object? Designer { get; set; }
    }

    private class NotificationItem
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

}
