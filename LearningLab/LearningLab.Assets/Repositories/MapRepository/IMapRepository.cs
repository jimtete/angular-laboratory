using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Assets.Repositories.MapRepository;

public interface IMapRepository
{
    Task<IReadOnlyList<Map>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<Map?> GetByIdAsync(
        int mapId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdInCampaignAsync(
        int mapId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Map map,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
