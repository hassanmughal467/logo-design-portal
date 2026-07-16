using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task CreateOrder_WithoutBearer_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders",
            new { title = "x", description = "y", price = 1 }, JsonOptions);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_AsDesigner_OnUnassignedOrder_Returns400Or403()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Authz test", description = "d", price = 50 }, JsonOptions);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(JsonOptions);
        Assert.NotNull(order);

        var designer = _factory.CreateClient();
        var designerToken = await AuthHelper.GetDesignerTokenAsync(designer);
        designer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", designerToken);

        var response = await designer.PutAsJsonAsync($"/api/orders/{order!.Id}/status",
            new { status = "InProgress", notes = "not assigned" }, JsonOptions);

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
            new { title = "Private", description = "d", price = 10 }, JsonOptions);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(JsonOptions);

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
    public async Task UpdateOrderStatus_AsOtherClient_Returns403()
    {
        var otherClientEmail = $"other-client-{Guid.NewGuid():N}@test.com";
        var orderId = await SeedPreviewDeliveredOrderForDefaultClientAsync(otherClientEmail);

        var otherClient = _factory.CreateClient();
        var otherClientToken = await AuthHelper.GetAccessTokenAsync(otherClient, otherClientEmail, "Test@123");
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherClientToken);

        var response = await otherClient.PutAsJsonAsync($"/api/orders/{orderId}/status",
            new { status = OrderStatus.ClientApproved.ToString(), notes = "cross-tenant attempt" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.LogoOrders.SingleAsync(o => o.Id == orderId);
        Assert.Equal(OrderStatus.PreviewDelivered, order.Status);
    }

    [Fact]
    public async Task SendPreviewBatch_AsClient_Returns403()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Batch", description = "d", price = 10 }, JsonOptions);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdResponse>(JsonOptions);

        var response = await client.PostAsJsonAsync($"/api/orders/{order!.Id}/send-preview-batch",
            new { previewBatchId = Guid.NewGuid() }, JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> SeedPreviewDeliveredOrderForDefaultClientAsync(string otherClientEmail)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clientRole = await context.Roles.SingleAsync(r => r.Id == SeededRoleIds.Client);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123", BCrypt.Net.BCrypt.GenerateSalt(10));

        var otherClientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = otherClientEmail,
            FirstName = "Other",
            LastName = "Client",
            PasswordHash = passwordHash,
            RoleId = clientRole.Id,
            Role = clientRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var otherClientProfile = new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = otherClientUser.Id,
            CompanyName = "Other Client Company",
            ContactName = "Other Client",
            CreatedAt = DateTime.UtcNow
        };
        var order = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = _factory.ClientProfileId,
            Title = "Cross-tenant status order",
            Description = "Owned by the seeded default client.",
            Status = OrderStatus.PreviewDelivered,
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(otherClientUser);
        context.ClientProfiles.Add(otherClientProfile);
        context.LogoOrders.Add(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    private sealed class OrderIdResponse
    {
        public Guid Id { get; set; }
    }
}
