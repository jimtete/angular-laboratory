using LearningLab.Assets.Models.DTOs;

namespace LearningLab.Assets.Repositories.AssetQueryRepository;

public interface IAssetQueryRepository
{
    Task<IReadOnlyList<AssetResponse>> GetAvailableItemsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);
}
