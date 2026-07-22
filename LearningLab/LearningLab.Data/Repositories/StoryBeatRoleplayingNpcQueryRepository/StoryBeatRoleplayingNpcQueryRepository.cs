using System.Data;
using Dapper;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBeatRoleplayingNpcQueryRepository;

public sealed class StoryBeatRoleplayingNpcQueryRepository : IStoryBeatRoleplayingNpcQueryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StoryBeatRoleplayingNpcQueryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CampaignNpcResponse>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            StoredProcedureNames.Gameplay.GetCampaignNpcsByCampaignId,
            new
            {
                CampaignId = campaignId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var npcs = await connection.QueryAsync<CampaignNpcRow>(command);

        return npcs
            .Select(npc => new CampaignNpcResponse
            {
                CampaignNpcId = npc.CampaignNpcId,
                CampaignId = npc.CampaignId,
                Tag = npc.Tag,
                Name = npc.Name,
                DisplayName = string.IsNullOrWhiteSpace(npc.DisplayName)
                    ? npc.Name
                    : npc.DisplayName,
                Description = npc.Description,
                CreatedAt = npc.CreatedAt,
                UpdatedAt = npc.UpdatedAt
            })
            .ToList();
    }

    private sealed class CampaignNpcRow
    {
        public Guid CampaignNpcId { get; init; }

        public Guid CampaignId { get; init; }

        public string Tag { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string? DisplayName { get; init; }

        public string Description { get; init; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }
    }
}
