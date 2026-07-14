namespace LearningLab.Data.Models.Notifications;

public sealed class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public NotificationType NotificationType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset DateCreated { get; set; }

    public DateTimeOffset? DateRead { get; set; }

    public DateTimeOffset? DateDeleted { get; set; }
}
