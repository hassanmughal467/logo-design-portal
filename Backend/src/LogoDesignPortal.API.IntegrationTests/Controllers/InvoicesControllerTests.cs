using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class InvoicesControllerTests
{
    private readonly TestWebApplicationFactory _factory;

    public InvoicesControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetInvoices_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/invoices");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoices_AsAdmin_Returns200()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/invoices");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvoice_WithCompletedBillableOrder_Returns201()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 199m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            orders = new[] { new { orderId, price = 199m } },
            billingType = BillingType.PerLogo,
            taxAmount = 0m
        };

        var response = await client.PostAsJsonAsync("/api/invoices", request, JsonOptions);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var invoice = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(invoice.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task CreateInvoice_EmptyOrdersAndNoManualItems_Returns400()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/invoices",
            new { billingType = BillingType.PerLogo }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInvoice_AsClient_Returns403()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 88m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/invoices",
            new { orders = new[] { new { orderId, price = 88m } }, billingType = BillingType.PerLogo, taxAmount = 0m },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetInvoices_WithInvalidJwt_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.bad");

        var response = await client.GetAsync("/api/invoices");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MarkPaid_AsAdmin_Returns200()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 129m);

        var client = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createResponse = await client.PostAsJsonAsync("/api/invoices", new
        {
            orders = new[] { new { orderId, price = 129m } },
            billingType = BillingType.PerLogo,
            taxAmount = 0m
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var invoiceId = created.GetProperty("id").GetGuid();

        var markPaid = await client.PutAsJsonAsync($"/api/invoices/{invoiceId}/mark-paid",
            new { paymentMethod = "BankTransfer" }, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, markPaid.StatusCode);
    }

    [Fact]
    public async Task MarkPaid_AsClient_Returns403()
    {
        var orderId = await IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync(
            _factory, _factory.ClientProfileId, 95m);

        var adminClient = _factory.CreateClient();
        var adminToken = await AuthHelper.GetSuperAdminTokenAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createResponse = await adminClient.PostAsJsonAsync("/api/invoices", new
        {
            orders = new[] { new { orderId, price = 95m } },
            billingType = BillingType.PerLogo,
            taxAmount = 0m
        }, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var invoiceId = created.GetProperty("id").GetGuid();

        var client = _factory.CreateClient();
        var clientToken = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientToken);
        var markPaid = await client.PutAsJsonAsync($"/api/invoices/{invoiceId}/mark-paid",
            new { paymentMethod = "BankTransfer" }, JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, markPaid.StatusCode);
    }
}
