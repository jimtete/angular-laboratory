using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Stores;

namespace LearningLab.Services.CampaignStoreService;

public interface ICampaignStoreService
{
    Task<ServiceResult<IReadOnlyList<StoreResponse>>> GetCampaignStoresAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoreResponse>> GetCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoreResponse>> CreateCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        CreateStoreRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoreResponse>> UpdateCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoreResponse>> UpdateCampaignStoreItemPurchasesAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreItemPurchaseStateRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoreResponse>> UpdateCampaignStoreLockStateAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreLockStateRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default);
}
