using Xunit;
using LogoDesignPortal.Application.Helpers;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Tests.Helpers;

/// <summary>
/// TEST-003: Verifies OrderLockingHelper correctly identifies terminal (locked) statuses.
/// </summary>
public class OrderLockingHelperTests
{
    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.CancelledByUser)]
    [InlineData(OrderStatus.CancelledByAdmin)]
    [InlineData(OrderStatus.Refunded)]
    public void IsOrderLocked_TerminalStatus_ReturnsTrue(OrderStatus status)
    {
        Assert.True(OrderLockingHelper.IsOrderLocked(status));
    }

    [Theory]
    [InlineData(OrderStatus.WaitingForAdminApproval)]
    [InlineData(OrderStatus.PriceApprovalPending)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.PreviewDelivered)]
    [InlineData(OrderStatus.RevisionRequested)]
    [InlineData(OrderStatus.ClientApproved)]
    public void IsOrderLocked_NonTerminalStatus_ReturnsFalse(OrderStatus status)
    {
        Assert.False(OrderLockingHelper.IsOrderLocked(status));
    }

    [Fact]
    public void LockedOrderMessage_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(OrderLockingHelper.LockedOrderMessage));
        Assert.Contains("cannot be modified", OrderLockingHelper.LockedOrderMessage);
    }
}
