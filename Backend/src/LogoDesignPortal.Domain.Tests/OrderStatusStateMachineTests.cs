using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.Domain.Tests;

/// <summary>Documents and enforces the order lifecycle state machine (domain rules).</summary>
public class OrderStatusStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.WaitingForAdminApproval, OrderStatus.InProgress)]
    [InlineData(OrderStatus.WaitingForAdminApproval, OrderStatus.PriceApprovalPending)]
    [InlineData(OrderStatus.InProgress, OrderStatus.PreviewDelivered)]
    [InlineData(OrderStatus.InProgress, OrderStatus.PriceApprovalPending)]
    [InlineData(OrderStatus.RevisionRequested, OrderStatus.PriceApprovalPending)]
    [InlineData(OrderStatus.PreviewDelivered, OrderStatus.ClientApproved)]
    [InlineData(OrderStatus.ClientApproved, OrderStatus.Completed)]
    [InlineData(OrderStatus.Completed, OrderStatus.Refunded)]
    [InlineData(OrderStatus.WaitingForAdminApproval, OrderStatus.CancelledByUser)]
    [InlineData(OrderStatus.PriceApprovalPending, OrderStatus.CancelledByUser)]
    public void ValidateTransition_AllowedPaths_DoesNotThrow(OrderStatus from, OrderStatus to)
    {
        var ex = Record.Exception(() => OrderStatusStateMachine.ValidateTransition(from, to));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateTransition_SameStatus_IsNoOp()
    {
        OrderStatusStateMachine.ValidateTransition(OrderStatus.InProgress, OrderStatus.InProgress);
    }

    [Theory]
    [InlineData(OrderStatus.Completed, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.InProgress)]
    [InlineData(OrderStatus.WaitingForAdminApproval, OrderStatus.Completed)]
    public void ValidateTransition_Disallowed_ThrowsInvalidOperation(OrderStatus from, OrderStatus to)
    {
        Assert.Throws<InvalidOperationException>(() =>
            OrderStatusStateMachine.ValidateTransition(from, to));
    }

    [Fact]
    public void IsTransitionAllowed_MatchesValidate_forSample()
    {
        Assert.True(OrderStatusStateMachine.IsTransitionAllowed(OrderStatus.PreviewDelivered, OrderStatus.RevisionRequested));
        Assert.False(OrderStatusStateMachine.IsTransitionAllowed(OrderStatus.Refunded, OrderStatus.InProgress));
    }
}
