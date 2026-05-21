using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class InvoicesControllerPrivacyTests
{
    private readonly TestWebApplicationFactory _factory;

    public InvoicesControllerPrivacyTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetInvoiceById_AsOtherClient_Returns404()
    {
        var otherInvoiceId = await IntegrationDatabaseHelper.InsertOtherClientInvoiceAsync(_factory, 88m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/invoices/{otherInvoiceId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoiceLogs_AsOtherClient_Returns404()
    {
        var otherInvoiceId = await IntegrationDatabaseHelper.InsertOtherClientInvoiceAsync(_factory, 55m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/invoices/{otherInvoiceId}/logs");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoiceById_AsDesigner_Returns404()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/invoices/{invoiceId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoices_AsDesigner_ReturnsEmptyList()
    {
        await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/invoices");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedInvoicesResponse>(IntegrationTestJson.Options);
        Assert.NotNull(body);
        Assert.Empty(body!.Items);
        Assert.Equal(0, body.Total);
    }

    [Fact]
    public async Task GetInvoiceById_AsOwningClient_Returns200()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/invoices/{invoiceId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<Guid> CreateInvoiceForSeededClientAsync()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 60m);

        var admin = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await admin.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 60m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var invoice = await create.Content.ReadFromJsonAsync<InvoiceIdResponse>(IntegrationTestJson.Options);
        return invoice!.Id;
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class PagedInvoicesResponse
    {
        public List<InvoiceIdResponse> Items { get; set; } = new();
        public int Total { get; set; }
    }
}
