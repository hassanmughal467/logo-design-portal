using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class PaymentsControllerPrivacyTests
{
    private readonly TestWebApplicationFactory _factory;

    public PaymentsControllerPrivacyTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetPayment_AsOtherClient_Returns404()
    {
        var paymentId = await SeedPaymentForOtherClientAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/payments/{paymentId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentsByInvoice_AsOtherClient_Returns404()
    {
        var otherInvoiceId = await IntegrationDatabaseHelper.InsertOtherClientInvoiceAsync(_factory, 77m);

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetClientTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/payments/invoice/{otherInvoiceId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> SeedPaymentForOtherClientAsync()
    {
        var otherInvoiceId = await IntegrationDatabaseHelper.InsertOtherClientInvoiceAsync(_factory, 42m);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var paymentId = Guid.NewGuid();
        db.Payments.Add(new Payment
        {
            Id = paymentId,
            InvoiceId = otherInvoiceId,
            PaymentMethod = "banktransfer",
            Amount = 42m,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return paymentId;
    }
}
