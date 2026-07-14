using LearningLab.Data.Models.Notifications;

namespace LearningLab.Data.Repositories.NotificationCommandRepository;

public interface INotificationCommandRepository
{
    Task CreateNotificationAsync(
        Guid notificationId,
        Guid userId,
        NotificationType notificationType,
        string description,
        DateTimeOffset dateCreated,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> SoftDeleteNotificationsAsync(
        Guid userId,
        NotificationType notificationType,
        string description,
        DateTimeOffset dateDeleted,
        CancellationToken cancellationToken = default);
}
