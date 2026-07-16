using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Assets;

namespace LearningLab.Services.AssetService;

public interface IAssetService
{
    Task<ServiceResult<IReadOnlyList<AssetResponse>>> GetAvailableItemsByCampaignIdAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<AssetResponse>>> GetAssetsAsync(
        int? parentAssetId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AssetResponse>> CreateFolderAsync(
        CreateAssetFolderRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AssetResponse>> CreateItemAsync(
        CreateItemAssetRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AssetResponse>> UpdateItemAsync(
        int assetId,
        UpdateItemAssetRequest request,
        CancellationToken cancellationToken = default);
}
