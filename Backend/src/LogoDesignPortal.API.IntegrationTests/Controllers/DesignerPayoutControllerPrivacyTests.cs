using System.Net;
using System.Net.Http.Headers;
using LogoDesignPortal.API.IntegrationTests.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using LogoDesignPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LogoDesignPortal.API.IntegrationTests.Controllers;

[Collection("Integration")]
public class DesignerPayoutControllerPrivacyTests
{
    private readonly TestWebApplicationFactory _factory;

    public DesignerPayoutControllerPrivacyTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetMyInvoice_AsDesigner_ForUnknownInvoice_Returns404()
    {
        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/designer-payout/me/invoices/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMyInvoice_AsDesigner_ForOtherDesignersInvoice_Returns404()
    {
        var otherInvoiceId = await SeedDesignerInvoiceForOtherDesignerAsync();

        var client = _factory.CreateClient();
        var token = await AuthHelper.GetDesignerTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/designer-payout/me/invoices/{otherInvoiceId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> SeedDesignerInvoiceForOtherDesignerAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var otherDesignerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = otherUserId,
            Email = $"other-designer-{otherUserId:N}@test.com",
            PasswordHash = "hash",
            RoleId = (await db.Roles.FirstAsync(r => r.Name == "Designer")).Id,
            CreatedAt = DateTime.UtcNow
        });
        db.DesignerProfiles.Add(new DesignerProfile
        {
            Id = otherDesignerId,
            UserId = otherUserId,
            CreatedAt = DateTime.UtcNow
        });

        var invoiceId = Guid.NewGuid();
        db.DesignerInvoices.Add(new DesignerInvoice
        {
            Id = invoiceId,
            DesignerId = otherDesignerId,
            InvoiceNumber = "DINV-OTHER",
            TotalAmount = 200m,
            Status = DesignerInvoiceStatus.Pending,
            BillingPeriod = "March 2026",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return invoiceId;
    }
}
