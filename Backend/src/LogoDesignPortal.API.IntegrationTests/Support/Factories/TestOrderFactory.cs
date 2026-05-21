using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.API.IntegrationTests.Support.Factories;

/// <summary>Builds <see cref="LogoOrder"/> rows for integration / API setup. Caller saves via EF.</summary>
public static class TestOrderFactory
{
    public static LogoOrder CreateBillableCompleted(
        Guid clientProfileId,
        decimal amount = 100m,
        Action<LogoOrder>? configure = null)
    {
        var o = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfileId,
            Title = "Factory order",
            Description = "Integration factory — billable completed order.",
            Status = OrderStatus.Completed,
            Priority = OrderPriority.Medium,
            Price = amount,
            ClientBasePrice = amount,
            ClientChargePrice = amount,
            BillingEligible = true,
            IsInvoiced = false,
            AllowUploads = false,
        };
        configure?.Invoke(o);
        return o;
    }

    public static LogoOrder CreatePendingApproval(Guid clientProfileId, Action<LogoOrder>? configure = null)
    {
        var o = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfileId,
            Title = "Pending approval",
            Description = "Awaiting admin approval — factory default.",
            Status = OrderStatus.WaitingForAdminApproval,
            Price = 50,
            ClientBasePrice = 50,
            ClientChargePrice = 50,
        };
        configure?.Invoke(o);
        return o;
    }

    public static LogoOrder CreatePreviewDelivered(Guid clientProfileId, decimal price = 80m, Action<LogoOrder>? configure = null)
    {
        var o = new LogoOrder
        {
            Id = Guid.NewGuid(),
            ClientId = clientProfileId,
            Title = "Preview delivered",
            Description = "Ready for client revision or approval.",
            Status = OrderStatus.PreviewDelivered,
            Priority = OrderPriority.Medium,
            Price = price,
            ClientBasePrice = price,
            ClientChargePrice = price,
            RevisionCount = 0,
            AllowUploads = false,
        };
        configure?.Invoke(o);
        return o;
    }
}
