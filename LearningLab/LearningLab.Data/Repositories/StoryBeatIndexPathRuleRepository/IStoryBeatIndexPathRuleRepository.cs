using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBeatIndexPathRuleRepository;

public interface IStoryBeatIndexPathRuleRepository
{
    Task<StoryBeatIndexPathRule?> GetByCampaignStoryBlockAndOrderIndexAsync(
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoryBeatIndexPathRule>> ListByStoryBlockAsync(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoryBeatIndexPathRule rule,
        CancellationToken cancellationToken = default);

    void Remove(StoryBeatIndexPathRule rule);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
