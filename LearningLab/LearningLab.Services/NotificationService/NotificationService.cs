using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Notifications;
using LearningLab.Data.Repositories.NotificationQueryRepository;

namespace LearningLab.Services.NotificationService;

public class NotificationService : INotificationService
{
    private readonly INotificationQueryRepository _notificationQueryRepository;

    public NotificationService(INotificationQueryRepository notificationQueryRepository)
    {
        _notificationQueryRepository = notificationQueryRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<NotificationResponse>>> GetNotificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = 
            await _notificationQueryRepository
                .GetAvailableByUserIdAsync(userId, cancellationToken);

        return new ServiceResult<IReadOnlyList<NotificationResponse>>(ApplicationStatusCode.Success, notifications);
    }
}