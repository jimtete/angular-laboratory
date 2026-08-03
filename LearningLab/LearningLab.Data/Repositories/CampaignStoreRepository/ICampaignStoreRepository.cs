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

    Task AddAsync(
        StoreEntry store,
        CancellationToken cancellationToken = default);

    void Remove(StoreEntry store);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
