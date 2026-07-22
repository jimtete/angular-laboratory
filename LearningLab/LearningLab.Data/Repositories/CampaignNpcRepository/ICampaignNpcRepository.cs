using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Repositories.CampaignNpcRepository;

public interface ICampaignNpcRepository
{
    Task<IReadOnlyList<CampaignNpc>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignNpc>> ListByCampaignIdAndTagsAsync(
        Guid campaignId,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default);

    Task<CampaignNpc?> GetByCampaignIdAndTagAsync(
        Guid campaignId,
        string tag,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCampaignIdAndTagAsync(
        Guid campaignId,
        string tag,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignNpc npc,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
