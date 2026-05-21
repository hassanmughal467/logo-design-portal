using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Regression;

[Collection("Integration")]
public class MarkPaidDuplicateRegressionTests
{
    private readonly TestWebApplicationFactory _factory;

    public MarkPaidDuplicateRegressionTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task MarkPaid_Twice_SecondCallFails()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 99m);

        var admin = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var create = await admin.PostAsJsonAsync("/api/invoices",
            new
            {
                orders = new[] { new { orderId, price = 99m } },
                billingType = BillingType.PerLogo,
                taxAmount = 0m
            },
            IntegrationTestJson.Options);
        create.EnsureSuccessStatusCode();
        var invoice = await create.Content.ReadFromJsonAsync<InvoiceIdResponse>(IntegrationTestJson.Options);

        var first = await admin.PutAsJsonAsync($"/api/invoices/{invoice!.Id}/mark-paid",
            new { paymentMethod = "BankTransfer" },
            IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await admin.PutAsJsonAsync($"/api/invoices/{invoice.Id}/mark-paid",
            new { paymentMethod = "BankTransfer" },
            IntegrationTestJson.Options);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    private sealed class InvoiceIdResponse
    {
        public Guid Id { get; set; }
    }
}
