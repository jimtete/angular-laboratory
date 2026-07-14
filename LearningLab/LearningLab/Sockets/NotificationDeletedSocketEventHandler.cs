using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets;

public sealed class NotificationDeletedSocketEventHandler
    : IApplicationEventHandler<NotificationDeletedEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationDeletedSocketEventHandler(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task HandleAsync(
        NotificationDeletedEvent applicationEvent,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(SocketGroupNames.UserNotifications(applicationEvent.UserId))
            .SendAsync(
                NotificationSocketClientEvents.NotificationDeleted,
                applicationEvent.NotificationIds,
                cancellationToken);
    }
}
