using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBlockRepository;

public interface IStoryBlockRepository
{
    Task<IReadOnlyList<StoryBlock>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<StoryBlock?> GetByCampaignIdAndStoryBlockIdAsync(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoryBlock storyBlock,
        CancellationToken cancellationToken = default);

    void Update(StoryBlock storyBlock);

    void Remove(StoryBlock storyBlock);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
