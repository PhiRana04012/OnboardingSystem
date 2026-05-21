using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OnboardingSystem.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userId, out var id))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(id));
        }

        await base.OnConnectedAsync();
    }

    public static string UserGroup(int userId) => $"user_{userId}";
}
