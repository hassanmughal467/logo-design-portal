using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

/// <summary>
/// Concurrent API calls against the same order (status / assign).
/// Documents last-write behavior; does not require row-version conflict on all paths.
/// </summary>
[Collection("Integration")]
public class OrdersControllerConcurrencyTests
{
    private readonly TestWebApplicationFactory _factory;

    public OrdersControllerConcurrencyTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task UpdateOrderStatus_ParallelInvalidAndValid_OneSucceedsOneFails()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Concurrency", description = "d", price = 40 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdDto>(IntegrationTestJson.Options);

        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(client);

        async Task<HttpResponseMessage> TryTransitionAsync(string status)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            return await http.PutAsJsonAsync($"/api/orders/{order!.Id}/status",
                new { status, notes = "parallel" }, IntegrationTestJson.Options);
        }

        var tasks = new[]
        {
            TryTransitionAsync("InProgress"),
            TryTransitionAsync("Completed")
        };

        var results = await Task.WhenAll(tasks);
        var codes = results.Select(r => r.StatusCode).ToList();

        Assert.Contains(HttpStatusCode.OK, codes);
        Assert.Contains(HttpStatusCode.BadRequest, codes);
    }

    [Fact]
    public async Task AssignDesigner_ParallelDuplicateAssign_BothReturnSuccessOrOneBadRequest()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Assign race", description = "d", price = 40 }, IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var order = await create.Content.ReadFromJsonAsync<OrderIdDto>(IntegrationTestJson.Options);

        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(client);
        var body = new { designerId = _factory.DesignerUserId };

        async Task<HttpResponseMessage> AssignAsync()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            return await http.PostAsJsonAsync($"/api/orders/{order!.Id}/assign", body, IntegrationTestJson.Options);
        }

        var results = await Task.WhenAll(AssignAsync(), AssignAsync());
        Assert.True(results.All(r => r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.BadRequest),
            string.Join(", ", results.Select(r => r.StatusCode)));
    }

    private sealed class OrderIdDto
    {
        public Guid Id { get; set; }
    }
}
