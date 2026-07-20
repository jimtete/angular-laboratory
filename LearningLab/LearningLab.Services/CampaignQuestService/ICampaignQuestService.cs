using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Quests;

namespace LearningLab.Services.CampaignQuestService;

public interface ICampaignQuestService
{
    Task<ServiceResult<IReadOnlyList<CampaignQuestResponse>>> GetCampaignQuestsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignQuestResponse>> CreateCampaignQuestAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignQuestRequest request,
        CancellationToken cancellationToken = default);
}
