using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class PaymentsControllerAuthorizationTests
{
    private readonly TestWebApplicationFactory _factory;

    public PaymentsControllerAuthorizationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPayment_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePaymentStatus_AsClient_Returns403()
    {
        var paymentId = await SeedPendingPaymentAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync($"/api/payments/{paymentId}/status",
            new { status = "Completed", transactionId = "manual" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePaymentLink_AsDesigner_ForClientInvoice_Returns403()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments/link",
            new { invoiceId, paymentMethod = "banktransfer" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_AsDesigner_ForClientInvoice_Returns403()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments",
            new { invoiceId, paymentMethod = "banktransfer", amount = 110m, currency = "USD" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateInvoiceForSeededClientAsync()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 110m);

        var admin = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await admin.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 110m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var invoice = await create.Content.ReadFromJsonAsync<InvoiceIdResponse>(IntegrationTestJson.Options);
        return invoice!.Id;
    }

    private async Task<Guid> SeedPendingPaymentAsync()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await client.PostAsJsonAsync("/api/payments",
            new
            {
                invoiceId,
                paymentMethod = "banktransfer",
                amount = 110m,
                currency = "USD"
            },
            IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var payment = await create.Content.ReadFromJsonAsync<PaymentIdResponse>(IntegrationTestJson.Options);
        return payment!.Id;
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class PaymentIdResponse
    {
        public Guid Id { get; set; }
    }
}
