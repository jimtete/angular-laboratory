using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Monsters;

namespace LearningLab.Data.Repositories.MonsterRepository;

public interface IMonsterRepository
{
    Task<IReadOnlyList<Monster>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Monster>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Monster>> ListDetailedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<Monster?> GetByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<Monster?> GetMutableByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<bool> CampaignParticipationExistsAsync(
        Guid campaignId,
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<int> CountCampaignParticipationsByMonsterIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> monsterIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Monster monster,
        CancellationToken cancellationToken = default);

    Task AddCampaignParticipationAsync(
        CampaignNpcParticipation participation,
        CancellationToken cancellationToken = default);

    void Remove(Monster monster);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
