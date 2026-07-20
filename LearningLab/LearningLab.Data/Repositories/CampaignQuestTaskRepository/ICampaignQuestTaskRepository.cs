using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Repositories.CampaignQuestTaskRepository;

public interface ICampaignQuestTaskRepository
{
    Task<IReadOnlyList<CampaignQuestTask>> ListByQuestIdAsync(
        Guid questId,
        CancellationToken cancellationToken = default);

    Task<CampaignQuestTask?> GetByQuestIdAndTaskIdAsync(
        Guid questId,
        Guid questTaskId,
        CancellationToken cancellationToken = default);

    Task<CampaignQuestTask?> GetByCampaignIdAndTaskIdAsync(
        Guid campaignId,
        Guid questTaskId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignQuestTask task,
        CancellationToken cancellationToken = default);

    void Update(CampaignQuestTask task);

    void Remove(CampaignQuestTask task);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
