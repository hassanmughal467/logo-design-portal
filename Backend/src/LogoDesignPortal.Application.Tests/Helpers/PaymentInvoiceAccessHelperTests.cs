using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Domain.Entities;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class PaymentInvoiceAccessHelperTests
{
    [Fact]
    public void CanAccessInvoice_ClientOwns_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Client = new ClientProfile { UserId = userId }
        };

        Assert.True(PaymentInvoiceAccessHelper.CanAccessInvoice(invoice, userId, "Client"));
    }

    [Fact]
    public void CanAccessInvoice_ClientOther_ReturnsFalse()
    {
        var invoice = new Invoice
        {
            Client = new ClientProfile { UserId = Guid.NewGuid() }
        };

        Assert.False(PaymentInvoiceAccessHelper.CanAccessInvoice(invoice, Guid.NewGuid(), "Client"));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    public void CanAccessInvoice_AdminRoles_ReturnsTrue(string role)
    {
        var invoice = new Invoice
        {
            Client = new ClientProfile { UserId = Guid.NewGuid() }
        };

        Assert.True(PaymentInvoiceAccessHelper.CanAccessInvoice(invoice, Guid.NewGuid(), role));
    }

    [Fact]
    public void CanAccessInvoice_Designer_ReturnsFalse()
    {
        var invoice = new Invoice
        {
            Client = new ClientProfile { UserId = Guid.NewGuid() }
        };

        Assert.False(PaymentInvoiceAccessHelper.CanAccessInvoice(invoice, Guid.NewGuid(), "Designer"));
    }
}
