using System.Data;
using Dapper;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.DTOs.Notifications;

namespace LearningLab.Data.Repositories.NotificationQueryRepository;

public sealed class NotificationQueryRepository : INotificationQueryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationQueryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetAvailableByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            StoredProcedureNames.Platform.GetAvailableNotificationsByUserId,
            new
            {
                UserId = userId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var notifications = await connection.QueryAsync<NotificationResponse>(command);

        return notifications.AsList();
    }
}
