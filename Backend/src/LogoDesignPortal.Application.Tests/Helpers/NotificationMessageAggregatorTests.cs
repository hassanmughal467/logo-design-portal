using LogoDesignPortal.Application.Helpers;
using Xunit;

namespace LogoDesignPortal.Application.Tests.Helpers;

public class NotificationMessageAggregatorTests
{
    [Fact]
    public void BuildAggregatedMessage_OrderCompleted_PluralizesFirstWord()
    {
        var result = NotificationMessageAggregator.BuildAggregatedMessage(
            "Order (#ORD-592244FB) has been completed",
            2);

        Assert.Equal("2 orders (#ORD-592244FB) has been completed", result);
    }

    [Fact]
    public void BuildAggregatedMessage_CountOne_ReturnsOriginal()
    {
        const string message = "Client approved the logo for order (#ORD-1).";
        Assert.Equal(message, NotificationMessageAggregator.BuildAggregatedMessage(message, 1));
    }
}
