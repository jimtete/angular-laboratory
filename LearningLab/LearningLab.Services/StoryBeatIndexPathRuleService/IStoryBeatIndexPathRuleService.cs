using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Services.StoryBeatIndexPathRuleService;

public interface IStoryBeatIndexPathRuleService
{
    Task<ServiceResult<IReadOnlyList<StoryBeatIndexPathRuleResponse>>> ListByStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatIndexPathRuleResponse>> GetByOrderIndexAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryBeatIndexPathRuleResponse>> UpsertAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        UpsertStoryBeatIndexPathRuleRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);
}
