using LearningLab.Data.Models.Assets;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.AssetRepository;

public sealed class AssetRepository : IAssetRepository
{
    private readonly LearningLabContext _context;

    public AssetRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Asset>> ListByParentAssetIdAsync(
        int? parentAssetId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(asset => asset.ParentAssetId == parentAssetId)
            .OrderBy(asset => asset.AssetType)
            .ThenBy(asset => asset.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Asset?> GetByIdAsync(
        int assetId,
        CancellationToken cancellationToken = default)
    {
        return _context.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                asset => asset.Id == assetId,
                cancellationToken);
    }

    public Task<Asset?> GetMutableByIdAsync(
        int assetId,
        CancellationToken cancellationToken = default)
    {
        return _context.Assets
            .SingleOrDefaultAsync(
                asset => asset.Id == assetId,
                cancellationToken);
    }

    public Task<bool> ExistsByParentAssetIdAndNameAsync(
        int? parentAssetId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _context.Assets
            .AsNoTracking()
            .AnyAsync(
                asset => asset.ParentAssetId == parentAssetId
                    && asset.Name == name,
                cancellationToken);
    }

    public Task<bool> ExistsByParentAssetIdAndNameExcludingAssetIdAsync(
        int? parentAssetId,
        string name,
        int assetId,
        CancellationToken cancellationToken = default)
    {
        return _context.Assets
            .AsNoTracking()
            .AnyAsync(
                asset => asset.ParentAssetId == parentAssetId
                    && asset.Name == name
                    && asset.Id != assetId,
                cancellationToken);
    }

    public async Task AddAsync(
        Asset asset,
        CancellationToken cancellationToken = default)
    {
        await _context.Assets.AddAsync(asset, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
