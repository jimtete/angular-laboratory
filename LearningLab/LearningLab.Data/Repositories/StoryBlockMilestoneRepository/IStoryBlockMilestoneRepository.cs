using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBlockMilestoneRepository;

public interface IStoryBlockMilestoneRepository
{
    Task<IReadOnlyList<StoryBlockMilestone>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<StoryBlockMilestone?> GetByStoryBlockIdAndCampaignMilestoneIdAsync(
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default);

    Task<StoryBlockMilestone?> GetByCampaignMilestoneIdAsync(
        int campaignMilestoneId,
        CancellationToken cancellationToken = default);

    Task<int?> GetLatestOrderIndexByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoryBlockMilestone storyBlockMilestone,
        CancellationToken cancellationToken = default);

    Task DecrementOrderAfterAsync(
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);

    void Remove(StoryBlockMilestone storyBlockMilestone);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
