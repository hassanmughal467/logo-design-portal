using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class OrderStatusTransitionHelperTests
{
    [Fact]
    public void ApplyClientCancellation_FromWaitingForAdminApproval_SetsCancelledByUser()
    {
        var order = new LogoOrder { Status = OrderStatus.WaitingForAdminApproval };
        OrderStatusTransitionHelper.ApplyClientCancellation(order);
        Assert.Equal(OrderStatus.CancelledByUser, order.Status);
    }

    [Fact]
    public void ApplyAdminCancellation_FromPreviewDelivered_SetsCancelledByAdmin()
    {
        var order = new LogoOrder { Status = OrderStatus.PreviewDelivered };
        OrderStatusTransitionHelper.ApplyAdminCancellation(order);
        Assert.Equal(OrderStatus.CancelledByAdmin, order.Status);
    }
}
