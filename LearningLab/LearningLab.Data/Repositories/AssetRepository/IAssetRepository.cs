using LearningLab.Data.Models.Assets;

namespace LearningLab.Data.Repositories.AssetRepository;

public interface IAssetRepository
{
    Task<IReadOnlyList<Asset>> ListByParentAssetIdAsync(
        int? parentAssetId,
        CancellationToken cancellationToken = default);

    Task<Asset?> GetByIdAsync(
        int assetId,
        CancellationToken cancellationToken = default);

    Task<Asset?> GetMutableByIdAsync(
        int assetId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByParentAssetIdAndNameAsync(
        int? parentAssetId,
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByParentAssetIdAndNameExcludingAssetIdAsync(
        int? parentAssetId,
        string name,
        int assetId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Asset asset,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
