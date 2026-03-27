using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.Domain.Tests;

/// <summary>Entity defaults express implicit business invariants at construction time.</summary>
public class EntityDefaultsTests
{
    [Fact]
    public void LogoOrder_NewInstance_HasExpectedDomainDefaults()
    {
        var order = new LogoOrder();

        Assert.Equal(OrderStatus.WaitingForAdminApproval, order.Status);
        Assert.Equal(OrderPriority.Medium, order.Priority);
        Assert.Equal(PriceApprovalStatus.NotSubmitted, order.PriceApprovalStatus);
        Assert.Equal("USD", order.CurrencyCode);
        Assert.False(order.RequiresPriceApproval);
        Assert.False(order.PriceApproved);
        Assert.False(order.IsCancelledByUser);
        Assert.False(order.IsArchived);
        Assert.False(order.IsRefunded);
        Assert.True(order.AllowUploads);
        Assert.False(order.IsInvoiced);
        Assert.False(order.IsDesignerInvoiced);
    }

    [Fact]
    public void Invoice_NewInstance_HasExpectedDomainDefaults()
    {
        var invoice = new Invoice();

        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        Assert.Equal(BillingType.PerLogo, invoice.BillingType);
        Assert.NotEqual(default, invoice.IssueDate);
    }

    [Fact]
    public void BaseEntity_NewInstance_SoftDeleteDefaults()
    {
        var entity = new TestEntity();

        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.Null(entity.DeletedBy);
        Assert.NotEqual(default, entity.CreatedAt);
    }

    private sealed class TestEntity : BaseEntity;
}
