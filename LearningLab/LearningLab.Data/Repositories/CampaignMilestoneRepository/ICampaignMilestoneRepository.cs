using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Repositories.CampaignMilestoneRepository;

public interface ICampaignMilestoneRepository
{
    Task<IReadOnlyList<CampaignMilestone>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignMilestone>> ListUnachievedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignMilestone>> ListUnlinkedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<CampaignMilestone?> GetByCampaignIdAndMilestoneIdAsync(
        Guid campaignId,
        int milestoneId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignMilestone milestone,
        CancellationToken cancellationToken = default);

    void Remove(CampaignMilestone milestone);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
