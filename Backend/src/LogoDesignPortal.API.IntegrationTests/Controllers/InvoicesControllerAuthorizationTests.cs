using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class InvoicesControllerAuthorizationTests
{
    private readonly TestWebApplicationFactory _factory;

    public InvoicesControllerAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetInvoices_WithoutBearer_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/invoices");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvoice_AsDesigner_Returns403()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 120m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 120m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SendInvoice_AsClient_Returns403()
    {
        var invoiceId = await CreateInvoiceAsAdminAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync($"/api/invoices/{invoiceId}/send", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateInvoice_AsClient_Returns403()
    {
        var invoiceId = await CreateInvoiceAsAdminAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync($"/api/invoices/{invoiceId}",
            new { notes = "client edit attempt" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateInvoiceAsAdminAsync()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 75m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 75m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<InvoiceIdResponse>(IntegrationTestJson.Options);
        return created!.Id;
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }
}
