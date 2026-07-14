using LearningLab.Data.Models.Notifications;

namespace LearningLab.Data.Models.DTOs.Notifications;

public sealed class NotificationResponse
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public NotificationType NotificationType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset DateCreated { get; set; }

    public DateTimeOffset? DateRead { get; set; }
}
