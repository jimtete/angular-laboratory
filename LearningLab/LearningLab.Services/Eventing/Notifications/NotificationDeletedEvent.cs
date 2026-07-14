namespace LearningLab.Services.Eventing.Notifications;

public sealed class NotificationDeletedEvent : IApplicationEvent
{
    public NotificationDeletedEvent(
        Guid userId,
        IReadOnlyList<Guid> notificationIds)
    {
        UserId = userId;
        NotificationIds = notificationIds;
    }

    public Guid UserId { get; }

    public IReadOnlyList<Guid> NotificationIds { get; }
}
