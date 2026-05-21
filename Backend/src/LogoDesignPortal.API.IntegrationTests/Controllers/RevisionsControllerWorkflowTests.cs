using System.Net;
using System.Net.Http.Headers;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class RevisionsControllerWorkflowTests
{
    private readonly TestWebApplicationFactory _factory;

    public RevisionsControllerWorkflowTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task RequestRevision_AsClient_OnPreviewOrder_Returns200AndUpdatesStatus()
    {
        var orderId = await IntegrationDatabaseHelper.InsertPreviewDeliveredOrderAsync(
            _factory, _factory.ClientProfileId, price: 80m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var form = MultipartTestHelper.CreateRevisionRequestForm();
        var response = await client.PostAsync($"/api/revisions/orders/{orderId}/request", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await db.LogoOrders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.RevisionRequested, order!.Status);
        Assert.Equal(1, order.RevisionCount);
    }
}
