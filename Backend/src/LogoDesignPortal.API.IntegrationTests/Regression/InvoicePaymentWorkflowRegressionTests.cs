using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Regression;

[Collection("Integration")]
public class InvoicePaymentWorkflowRegressionTests
{
    private readonly TestWebApplicationFactory _factory;

    public InvoicePaymentWorkflowRegressionTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task InvoicePaymentWorkflowRegression_CreateMarkPaidClientRead()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 145m);

        var admin = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var create = await admin.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 145m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);
        Assert.True(create.IsSuccessStatusCode, await create.Content.ReadAsStringAsync());
        var invoice = await create.Content.ReadFromJsonAsync<InvoiceIdResponse>(IntegrationTestJson.Options);

        var markPaid = await admin.PutAsJsonAsync($"/api/invoices/{invoice!.Id}/mark-paid",
            new { paymentMethod = "BankTransfer" },
            IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.OK, markPaid.StatusCode);

        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);

        var get = await client.GetAsync($"/api/invoices/{invoice.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var body = await get.Content.ReadAsStringAsync();
        Assert.Contains("Paid", body, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }
}
