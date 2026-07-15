using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Data.Repositories.CampaignParticipationInviteRepository;

public interface ICampaignParticipationInviteRepository
{
    Task<IReadOnlyList<CampaignParticipationInvite>> ListPendingByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListParticipantUsernamesByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignMemberInformationResponse>> ListParticipantInformationByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListInviteUsernamesByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<int> CountReservedPlayerSlotsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<CampaignParticipationInvite?> GetInviteAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PlayerCampaignParticipation?> GetParticipationByUsernameAsync(
        Guid campaignId,
        string username,
        CancellationToken cancellationToken = default);

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

    Task AddParticipationAsync(
        PlayerCampaignParticipation participation,
        CancellationToken cancellationToken = default);

    void RemoveInvite(CampaignParticipationInvite invite);

    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
