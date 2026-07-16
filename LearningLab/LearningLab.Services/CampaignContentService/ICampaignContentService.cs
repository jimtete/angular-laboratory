using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Services.CampaignContentService;

public interface ICampaignContentService
{
    Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetCampaignMilestonesAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetUnachievedCampaignMilestonesAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignMilestoneResponse>> CreateCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignMilestoneResponse>> UpdateCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int milestoneId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int milestoneId,
        CancellationToken cancellationToken = default);
}
