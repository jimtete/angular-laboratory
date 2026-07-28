using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;

namespace LearningLab.Services.CampaignPresentationService;

public interface ICampaignPresentationService
{
    Task<ServiceResult<CampaignPresentationResponse>> GetPresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignPresentationResponse>> InitiatePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignPresentationResponse>> PresentStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default);
}
