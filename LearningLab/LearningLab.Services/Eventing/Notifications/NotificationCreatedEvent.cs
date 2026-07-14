using LearningLab.Data.Models.DTOs.Notifications;

namespace LearningLab.Services.Eventing.Notifications;

public sealed class NotificationCreatedEvent : IApplicationEvent
{
    public NotificationCreatedEvent(NotificationResponse notification)
    {
        Notification = notification;
    }

    public NotificationResponse Notification { get; }

    public Guid UserId => Notification.UserId;
}
