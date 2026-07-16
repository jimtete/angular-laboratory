using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Repositories.CampaignRepository;

public interface ICampaignRepository
{
    Task<IReadOnlyList<Campaign>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Campaign>> GetByGameMasterIdAsync(
        Guid gameMasterId,
        CancellationToken cancellationToken = default);

    Task<Campaign?> GetByIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<Campaign?> GetByIdForGameMasterAsync(
        Guid campaignId,
        Guid gameMasterId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<int> CountByIdsAsync(
        IReadOnlyCollection<Guid> campaignIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Campaign campaign,
        CancellationToken cancellationToken = default);

    void Update(Campaign campaign);

    void Remove(Campaign campaign);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
