using LearningLab.Data.Models.AccessControl;
using LearningLab.Services.Helpers;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets.Notifications;

[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User is null)
        {
            Context.Abort();
            return;
        }

        var userId = SessionHelper.GetUserId(Context.User);

        if (userId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SocketGroupNames.UserNotifications(userId.Value));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User is not null)
        {
            var userId = SessionHelper.GetUserId(Context.User);

            if (userId is not null)
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    SocketGroupNames.UserNotifications(userId.Value));
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
