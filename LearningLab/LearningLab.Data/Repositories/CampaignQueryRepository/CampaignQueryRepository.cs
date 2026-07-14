using System.Data;
using Dapper;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Data.Repositories.CampaignQueryRepository;

public sealed class CampaignQueryRepository : ICampaignQueryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CampaignQueryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CampaignResponse>> GetByGameMasterIdAsync(
        Guid gameMasterId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            StoredProcedureNames.Platform.GetCampaignsByGameMasterId,
            new
            {
                GameMasterId = gameMasterId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var campaigns = await connection.QueryAsync<CampaignResponse>(command);

        return campaigns.AsList();
    }
}
