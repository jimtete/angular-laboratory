using System.Data;
using System.Text.Json;
using Dapper;
using LearningLab.Data.Infrastructure.Database;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Models.DTOs.Assets;

namespace LearningLab.Data.Repositories.AssetQueryRepository;

public sealed class AssetQueryRepository : IAssetQueryRepository
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDbConnectionFactory _connectionFactory;

    public AssetQueryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AssetResponse>> GetAvailableItemsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            StoredProcedureNames.Gameplay.GetAvailableItemsByCampaignId,
            new
            {
                CampaignId = campaignId
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var assets = await connection.QueryAsync<AssetRow>(command);

        return assets
            .Select(ToResponse)
            .ToList();
    }

    private static AssetResponse ToResponse(AssetRow row)
    {
        return new AssetResponse
        {
            Id = row.Id,
            ParentAssetId = row.ParentAssetId,
            AssetType = Enum.Parse<AssetType>(row.AssetType),
            Name = row.Name,
            Description = row.Description,
            ItemType = string.IsNullOrWhiteSpace(row.ItemType)
                ? null
                : Enum.Parse<ItemType>(row.ItemType),
            CampaignIds = DeserializeCampaignIds(row.CampaignIds),
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt
        };
    }

    private static List<Guid>? DeserializeCampaignIds(string? campaignIds)
    {
        return string.IsNullOrWhiteSpace(campaignIds)
            ? null
            : JsonSerializer.Deserialize<List<Guid>>(campaignIds, JsonSerializerOptions);
    }

    private sealed class AssetRow
    {
        public int Id { get; init; }
        public int? ParentAssetId { get; init; }
        public string AssetType { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ItemType { get; init; }
        public string? CampaignIds { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
