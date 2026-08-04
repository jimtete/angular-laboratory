using LearningLab.Data.Models.Campaign.Stores;

namespace LearningLab.Data.Repositories.CampaignStoreRepository;

public interface ICampaignStoreRepository
{
    Task<IReadOnlyList<StoreEntry>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<StoreEntry?> GetByCampaignIdAndStoreIdAsync(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default);

    Task<StoreEntry?> GetMutableByCampaignIdAndStoreIdAsync(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoreEntry>> ListUnlockedByCampaignIdAndStoreTypeAsync(
        Guid campaignId,
        StoreType storeType,
        int excludedStoreId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoreEntry store,
        CancellationToken cancellationToken = default);

    void Remove(StoreEntry store);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
