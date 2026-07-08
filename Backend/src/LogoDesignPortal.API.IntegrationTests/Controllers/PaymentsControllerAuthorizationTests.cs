using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task ProcessPayment_BankTransferAsClient_DoesNotMarkPaymentOrInvoicePaid()
    {
        var (paymentId, invoiceId) = await SeedPendingPaymentWithInvoiceAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments/process",
            new { paymentId, bankReference = "BT-CLIENT-123" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentIdResponse>(IntegrationTestJson.Options);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Processing.ToString(), paymentResponse!.Status);
        Assert.Equal("BT-CLIENT-123", paymentResponse.TransactionId);
        Assert.Null(paymentResponse.CompletedAt);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invoice = await context.Invoices.SingleAsync(i => i.Id == invoiceId);
        var payment = await context.Payments.SingleAsync(p => p.Id == paymentId);
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        Assert.Null(invoice.PaidDate);
        Assert.Equal(PaymentStatus.Processing, payment.Status);
    }

    [Fact]
    public async Task ProcessPayment_BankTransferWithoutReference_Returns400()
    {
        var paymentId = await SeedPendingPaymentAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments/process",
            new { paymentId, bankReference = "" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePaymentStatus_AsSuperAdmin_CompletesPaymentAndMarksInvoicePaid()
    {
        var (paymentId, invoiceId) = await SeedPendingPaymentWithInvoiceAsync();

        var admin = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await admin.PutAsJsonAsync($"/api/payments/{paymentId}/status",
            new { status = PaymentStatus.Completed.ToString(), transactionId = "BT-VERIFIED-123" },
            IntegrationTestJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentIdResponse>(IntegrationTestJson.Options);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Completed.ToString(), paymentResponse!.Status);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invoice = await context.Invoices.SingleAsync(i => i.Id == invoiceId);
        var payment = await context.Payments.SingleAsync(p => p.Id == paymentId);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidDate);
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("BT-VERIFIED-123", payment.TransactionId);
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
        var (paymentId, _) = await SeedPendingPaymentWithInvoiceAsync();
        return paymentId;
    }

    private async Task<(Guid PaymentId, Guid InvoiceId)> SeedPendingPaymentWithInvoiceAsync()
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
        return (payment!.Id, invoiceId);
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class PaymentIdResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
