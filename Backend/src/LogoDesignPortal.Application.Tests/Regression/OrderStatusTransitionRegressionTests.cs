using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Regression;

public class OrderStatusTransitionRegressionTests
{
    [Fact]
    public void Regression_ClientCancel_FromPriceApprovalPending_Succeeds()
    {
        var order = new LogoOrder { Status = OrderStatus.PriceApprovalPending };
        OrderStatusTransitionHelper.ApplyClientCancellation(order);
        Assert.Equal(OrderStatus.CancelledByUser, order.Status);
    }

    [Fact]
    public void Regression_AdminCancel_FromPreviewDelivered_Succeeds()
    {
        var order = new LogoOrder { Status = OrderStatus.PreviewDelivered };
        OrderStatusTransitionHelper.ApplyAdminCancellation(order);
        Assert.Equal(OrderStatus.CancelledByAdmin, order.Status);
    }

    [Fact]
    public void Regression_PreviewDelivered_ToRevisionRequested_Succeeds()
    {
        var order = new LogoOrder { Status = OrderStatus.PreviewDelivered };
        OrderStatusTransitionHelper.Apply(order, OrderStatus.RevisionRequested);
        Assert.Equal(OrderStatus.RevisionRequested, order.Status);
    }

    [Fact]
    public void Regression_ManualComplete_FromInProgress_Succeeds()
    {
        var order = new LogoOrder { Status = OrderStatus.InProgress };
        OrderStatusTransitionHelper.ApplyManualCompleted(order);
        Assert.Equal(OrderStatus.Completed, order.Status);
    }
}
