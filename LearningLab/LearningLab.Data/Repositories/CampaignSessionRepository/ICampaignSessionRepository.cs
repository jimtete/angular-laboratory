using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Repositories.CampaignSessionRepository;

public interface ICampaignSessionRepository
{
    Task<IReadOnlyList<CampaignSession>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<int?> GetLatestSessionNumberByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<CampaignSession?> GetByCampaignIdAndSessionIdAsync(
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignSession session,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
