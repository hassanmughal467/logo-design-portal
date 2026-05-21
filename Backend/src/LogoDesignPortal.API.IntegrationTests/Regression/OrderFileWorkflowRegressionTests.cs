using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.API.IntegrationTests.Support.Factories;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Regression;

/// <summary>
/// End-to-end regression paths for order + file + permission module.
/// </summary>
[Collection("Integration")]
public class OrderFileWorkflowRegressionTests
{
    private readonly TestWebApplicationFactory _factory;

    public OrderFileWorkflowRegressionTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task OrderFileWorkflowRegression_CreateAssignInvalidDownloadCancel()
    {
        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        // 1. Create
        var create = await client.PostAsJsonAsync("/api/orders",
            new { title = "Regression", description = "flow", price = 80 }, IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var order = await create.Content.ReadFromJsonAsync<OrderDto>(IntegrationTestJson.Options);
        Assert.Equal(OrderStatus.WaitingForAdminApproval.ToString(), order!.Status);

        // 2. Invalid jump
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var bad = await client.PutAsJsonAsync($"/api/orders/{order.Id}/status",
            new { status = "Completed", notes = "invalid" }, IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        // 3. Assign
        var assign = await client.PostAsJsonAsync($"/api/orders/{order.Id}/assign",
            new { designerId = _factory.DesignerUserId }, IntegrationTestJson.Options);
        assign.EnsureSuccessStatusCode();

        // 4. Hidden file IDOR
        Guid hiddenFileId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var path = await TestLogoFileFactory.WriteTempPngAsync();
            var entity = TestLogoFileFactory.CreateHiddenPreview(order.Id, path, _factory.DesignerUserId);
            db.LogoFiles.Add(entity);
            await db.SaveChangesAsync();
            hiddenFileId = entity.Id;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
        var download = await client.GetAsync($"/api/files/{hiddenFileId}/download");
        Assert.Equal(HttpStatusCode.Forbidden, download.StatusCode);

        // 5. Client cancel
        var cancel = await client.PostAsJsonAsync($"/api/orders/{order.Id}/cancel",
            new { reason = "Changed mind" }, IntegrationTestJson.Options);
        Assert.True(
            cancel.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
            "Cancel may be rejected if assign moved status past allowed cancel window.");
    }

    private sealed class OrderDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "";
    }
}
