using LearningLab.Data.Models.DTOs.Assets;

namespace LearningLab.Data.Repositories.AssetQueryRepository;

public interface IAssetQueryRepository
{
    Task<IReadOnlyList<AssetResponse>> GetAvailableItemsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);
}
