using LearningLab.Data.Models.DTOs.Notifications;

namespace LearningLab.Data.Repositories.NotificationQueryRepository;

public interface INotificationQueryRepository
{
    Task<IReadOnlyList<NotificationResponse>> GetAvailableByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
