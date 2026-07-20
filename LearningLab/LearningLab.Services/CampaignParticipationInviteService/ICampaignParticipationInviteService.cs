using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Services.CampaignParticipationInviteService;

public interface ICampaignParticipationInviteService
{
    Task<ServiceResult<IReadOnlyList<CampaignPendingInviteResponse>>> GetPendingInvitesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<string>>> GetCampaignMemberUsernamesAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>> GetCampaignMemberInformationAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<string>>> GetCampaignInviteUsernamesAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignParticipationInviteResponse>> InvitePlayerAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignParticipationInviteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignInviteResolutionResponse>> AcceptInviteAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignInviteResolutionResponse>> RejectInviteAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);
}
