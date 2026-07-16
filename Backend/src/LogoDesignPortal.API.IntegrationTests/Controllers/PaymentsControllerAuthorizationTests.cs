using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Constants;
using LogoDesignPortal.Domain.Entities;
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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

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
            JsonOptions);

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
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentIdResponse>(JsonOptions);
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
            JsonOptions);

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
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentIdResponse>(JsonOptions);
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
            JsonOptions);

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
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_AsOtherClient_ForClientInvoice_Returns403()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();
        var otherClientEmail = $"payment-other-client-{Guid.NewGuid():N}@test.com";
        await SeedOtherClientAsync(otherClientEmail);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetAccessTokenAsync(client, otherClientEmail, "Test@123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments",
            new { invoiceId, paymentMethod = "banktransfer", amount = 110m, currency = "USD" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_AsAdmin_ForClientInvoice_ReturnsCreated()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var admin = _factory.CreateClient();
        var token = await AuthHelper.GetAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await admin.PostAsJsonAsync("/api/payments",
            new { invoiceId, paymentMethod = "banktransfer", amount = 110m, currency = "USD" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_AsSuperAdmin_ForClientInvoice_ReturnsCreated()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var admin = _factory.CreateClient();
        var token = await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await admin.PostAsJsonAsync("/api/payments",
            new { invoiceId, paymentMethod = "banktransfer", amount = 110m, currency = "USD" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePaymentLink_AsOtherClient_ForClientInvoice_Returns403()
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();
        var otherClientEmail = $"payment-link-other-client-{Guid.NewGuid():N}@test.com";
        await SeedOtherClientAsync(otherClientEmail);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetAccessTokenAsync(client, otherClientEmail, "Test@123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/payments/link",
            new { invoiceId, paymentMethod = "banktransfer" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    public async Task GeneratePaymentLink_AsAdminRoles_ForClientInvoice_ReturnsOk(string role)
    {
        var invoiceId = await CreateInvoiceForSeededClientAsync();

        var admin = _factory.CreateClient();
        var token = role == "Admin"
            ? await AuthHelper.GetAdminTokenAsync(admin)
            : await AuthHelper.GetSuperAdminTokenAsync(admin);
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await admin.PostAsJsonAsync("/api/payments/link",
            new { invoiceId, paymentMethod = "banktransfer" },
            JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<Guid> CreateInvoiceForSeededClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = await context.ClientProfiles.SingleAsync(c => c.Id == _factory.ClientProfileId);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            InvoiceNumber = $"PAY-AUTH-{Guid.NewGuid():N}",
            Amount = 110m,
            TaxAmount = 0m,
            TotalAmount = 110m,
            Status = InvoiceStatus.Pending,
            BillingType = BillingType.PerLogo,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        return invoice.Id;
    }

    private async Task SeedOtherClientAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var clientRole = await context.Roles.SingleAsync(r => r.Id == SeededRoleIds.Client);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123", BCrypt.Net.BCrypt.GenerateSalt(10));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Other",
            LastName = "PaymentClient",
            PasswordHash = passwordHash,
            RoleId = clientRole.Id,
            Role = clientRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.ClientProfiles.Add(new ClientProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CompanyName = "Other Payment Client",
            ContactName = "Other Payment Client",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
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
            JsonOptions);
        create.EnsureSuccessStatusCode();
        var payment = await create.Content.ReadFromJsonAsync<PaymentIdResponse>(JsonOptions);
        return (payment!.Id, invoiceId);
    }

    private sealed class PaymentIdResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
