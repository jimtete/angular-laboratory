using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets;

public sealed class NotificationCreatedSocketEventHandler
    : IApplicationEventHandler<NotificationCreatedEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotificationCreatedSocketEventHandler(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task HandleAsync(
        NotificationCreatedEvent applicationEvent,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(SocketGroupNames.UserNotifications(applicationEvent.UserId))
            .SendAsync(
                NotificationSocketClientEvents.NotificationCreated,
                applicationEvent.Notification,
                cancellationToken);
    }
}
