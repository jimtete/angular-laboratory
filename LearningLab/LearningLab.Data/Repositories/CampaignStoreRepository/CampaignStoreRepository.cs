using LearningLab.Data.Models.Campaign.Stores;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignStoreRepository;

public sealed class CampaignStoreRepository : ICampaignStoreRepository
{
    private readonly LearningLabContext _context;

    public CampaignStoreRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoreEntry>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await QueryStoresWithItems()
            .AsNoTracking()
            .Where(store => store.CampaignId == campaignId)
            .OrderBy(store => store.StoreType)
            .ThenBy(store => store.StoreName)
            .ThenBy(store => store.StoreLocation)
            .ThenBy(store => store.StoreId)
            .ToListAsync(cancellationToken);
    }

    public Task<StoreEntry?> GetByCampaignIdAndStoreIdAsync(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default)
    {
        return QueryStoresWithItems()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                store => store.CampaignId == campaignId
                    && store.StoreId == storeId,
                cancellationToken);
    }

    public Task<StoreEntry?> GetMutableByCampaignIdAndStoreIdAsync(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default)
    {
        return QueryStoresWithItems()
            .SingleOrDefaultAsync(
                store => store.CampaignId == campaignId
                    && store.StoreId == storeId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StoreEntry>> ListUnlockedByCampaignIdAndStoreTypeAsync(
        Guid campaignId,
        StoreType storeType,
        int excludedStoreId,
        CancellationToken cancellationToken = default)
    {
        return await QueryStoresWithItems()
            .AsNoTracking()
            .Where(store => store.CampaignId == campaignId
                && store.StoreType == storeType
                && store.StoreId != excludedStoreId
                && store.LockState == StoreLockState.Unlocked)
            .OrderBy(store => store.StoreName)
            .ThenBy(store => store.StoreLocation)
            .ThenBy(store => store.StoreId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        StoreEntry store,
        CancellationToken cancellationToken = default)
    {
        await _context.StoreEntries.AddAsync(store, cancellationToken);
    }

    public void Remove(StoreEntry store)
    {
        _context.StoreEntries.Remove(store);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<StoreEntry> QueryStoresWithItems()
    {
        return _context.StoreEntries
            .Include(store => store.Items);
    }
}
