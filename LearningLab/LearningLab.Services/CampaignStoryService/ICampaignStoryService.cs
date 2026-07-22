using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Services.CampaignStoryService;

public interface ICampaignStoryService
{
    Task<ServiceResult<StoryBlockResponse>> CreateStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        CreateStoryBlockRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryBlockResponse>>> GetStoryBlocksAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBlockResponse>> UpdateStoryBlockTitleAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        UpdateStoryBlockTitleRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBlockMilestoneResponse>> AddStoryBlockMilestoneAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>>> GetStoryBlockMilestonesAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetAvailableStoryBlockMilestonesAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> RemoveStoryBlockMilestoneAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> CreateInformationStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateInformationStoryBeatRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> CreateNarrativeStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateNarrativeStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> CreateRoleplayingStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> CreateDecisionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateDecisionStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> CreateMilestoneStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateMilestoneStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> UpdateInformationStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateInformationStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> UpdateNarrativeStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateNarrativeStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> UpdateRoleplayingStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> UpdateDecisionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateDecisionStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatResponse>> UpdateMilestoneStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateMilestoneStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryBeatResponse>>> GetStoryBeatsAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignNpcResponse>>> GetRoleplayingStoryBeatNpcsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignNpcResponse>> CreateCampaignNpcAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignNpcRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignNpcResponse>> UpdateCampaignNpcAsync(
        Guid userId,
        Guid campaignId,
        string npcTag,
        UpdateCampaignNpcRequest? request,
        CancellationToken cancellationToken = default);
}
