using System.Data;
using Dapper;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LearningLab.Data.Repositories.NotificationCommandRepository;

public sealed class NotificationCommandRepository : INotificationCommandRepository
{
    private readonly LearningLabContext _context;

    public NotificationCommandRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(
        Guid notificationId,
        Guid userId,
        NotificationType notificationType,
        string description,
        DateTimeOffset dateCreated,
        CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var command = new CommandDefinition(
            StoredProcedureNames.Platform.CreateNotification,
            new
            {
                NotificationId = notificationId,
                UserId = userId,
                NotificationType = notificationType.ToString(),
                Description = description,
                DateCreated = dateCreated
            },
            transaction: _context.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<IReadOnlyList<Guid>> SoftDeleteNotificationsAsync(
        Guid userId,
        NotificationType notificationType,
        string description,
        DateTimeOffset dateDeleted,
        CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var command = new CommandDefinition(
            """
            UPDATE [dbo].[Notifications]
            SET [date_deleted] = @DateDeleted
            OUTPUT inserted.[notification_id]
            WHERE [user_id] = @UserId
                AND [notification_type] = @NotificationType
                AND [description] = @Description
                AND [date_deleted] IS NULL;
            """,
            new
            {
                UserId = userId,
                NotificationType = notificationType.ToString(),
                Description = description,
                DateDeleted = dateDeleted
            },
            transaction: _context.Database.CurrentTransaction?.GetDbTransaction(),
            cancellationToken: cancellationToken);

        var deletedNotificationIds = await connection.QueryAsync<Guid>(command);

        return deletedNotificationIds.AsList();
    }
}
