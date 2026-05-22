using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LogoDesignPortal.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        if (!string.Equals(Context.UserIdentifier, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new HubException("You can only join your own notification group.");
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
    }
}
