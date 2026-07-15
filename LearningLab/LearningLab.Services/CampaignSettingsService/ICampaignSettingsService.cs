using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Services.CampaignSettingsService;

public interface ICampaignSettingsService
{
    Task<ServiceResult<CampaignInformationResponse>> GetCampaignInformationAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSettingsResponse>> GetCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSettingsResponse>> UpdateCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        UpdateCampaignSettingsRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignMemberInformationResponse>> UpdateCampaignMemberNicknameAsync(
        Guid userId,
        Guid campaignId,
        string username,
        UpdateCampaignMemberNicknameRequest request,
        CancellationToken cancellationToken = default);
}
