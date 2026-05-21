using LogoDesignPortal.Domain.Enums;
using Xunit;

namespace LogoDesignPortal.Domain.Tests;

/// <summary>
/// Regression matrix for invalid and terminal transitions.
/// </summary>
public class OrderStatusStateMachineRegressionTests
{
    public static IEnumerable<object[]> InvalidTransitions =>
        new List<object[]>
        {
            new object[] { OrderStatus.WaitingForAdminApproval, OrderStatus.Completed },
            new object[] { OrderStatus.WaitingForAdminApproval, OrderStatus.PreviewDelivered },
            new object[] { OrderStatus.InProgress, OrderStatus.Completed },
            new object[] { OrderStatus.PreviewDelivered, OrderStatus.InProgress },
            new object[] { OrderStatus.Completed, OrderStatus.InProgress },
            new object[] { OrderStatus.Refunded, OrderStatus.Completed },
            new object[] { OrderStatus.CancelledByAdmin, OrderStatus.InProgress },
        };

    [Theory]
    [MemberData(nameof(InvalidTransitions))]
    public void ValidateTransition_RegressionInvalidMatrix_Throws(OrderStatus from, OrderStatus to)
    {
        Assert.Throws<InvalidOperationException>(() =>
            OrderStatusStateMachine.ValidateTransition(from, to));
    }

    [Theory]
    [InlineData(OrderStatus.PreviewDelivered, OrderStatus.RevisionRequested)]
    [InlineData(OrderStatus.RevisionRequested, OrderStatus.PreviewDelivered)]
    [InlineData(OrderStatus.ClientApproved, OrderStatus.Completed)]
    public void ValidateTransition_RegressionValidWorkflow_DoesNotThrow(OrderStatus from, OrderStatus to)
    {
        OrderStatusStateMachine.ValidateTransition(from, to);
    }
}
