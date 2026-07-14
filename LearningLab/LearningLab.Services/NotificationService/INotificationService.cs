using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Notifications;

namespace LearningLab.Services.NotificationService;

public interface INotificationService
{
    Task<ServiceResult<IReadOnlyList<NotificationResponse>>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}