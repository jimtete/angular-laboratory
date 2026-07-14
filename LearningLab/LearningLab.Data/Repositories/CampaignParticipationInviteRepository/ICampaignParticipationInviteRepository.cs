using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Repositories.CampaignParticipationInviteRepository;

public interface ICampaignParticipationInviteRepository
{
    Task<bool> ExistsInviteAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsParticipationAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignParticipationInvite invite,
        CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
