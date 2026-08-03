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

    Task<ServiceResult<CampaignQuestResponse>> UpdateCampaignQuestAsync(
        Guid userId,
        Guid campaignId,
        Guid questId,
        UpdateCampaignQuestRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>> DeleteCampaignQuestAsync(
        Guid userId,
        Guid campaignId,
        Guid questId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>> GetStoryBeatQuestTasksAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>> GetCampaignStoryBeatQuestTasksAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatQuestTaskResponse>> LinkQuestTaskToStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> UnlinkQuestTaskFromStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default);
}
