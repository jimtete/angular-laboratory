using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Repositories.CampaignQuestRepository;

public interface ICampaignQuestRepository
{
    Task<IReadOnlyList<CampaignQuest>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignQuest>> ListIncompleteByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<CampaignQuest?> GetByCampaignIdAndQuestIdAsync(
        Guid campaignId,
        Guid questId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignQuest quest,
        CancellationToken cancellationToken = default);

    void Update(CampaignQuest quest);

    void Remove(CampaignQuest quest);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
