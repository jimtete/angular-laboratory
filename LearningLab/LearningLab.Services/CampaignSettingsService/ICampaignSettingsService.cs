using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Services.CampaignSettingsService;

public interface ICampaignSettingsService
{
    Task<ServiceResult<CampaignSettingsResponse>> GetCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSettingsResponse>> UpdateCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        UpdateCampaignSettingsRequest request,
        CancellationToken cancellationToken = default);
}
