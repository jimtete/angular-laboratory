using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Services.CampaignParticipationInviteService;

public interface ICampaignParticipationInviteService
{
    Task<ServiceResult<CampaignParticipationInviteResponse>> InvitePlayerAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignParticipationInviteRequest request,
        CancellationToken cancellationToken = default);
}
