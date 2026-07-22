using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBeatRepository;

public interface IStoryBeatRepository
{
    Task<IReadOnlyList<StoryBeat>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<StoryBeat?> GetByStoryBlockIdAndStoryBeatIdAsync(
        Guid storyBlockId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<StoryBeat?> GetByCampaignIdAndStoryBeatIdAsync(
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<StoryBeat?> GetByCampaignIdAndCampaignMilestoneIdAsync(
        Guid campaignId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default);

    Task<int?> GetLatestOrderIndexByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoryBeat storyBeat,
        CancellationToken cancellationToken = default);

    Task DecrementOrderAfterAsync(
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);

    void Update(StoryBeat storyBeat);

    void Remove(StoryBeat storyBeat);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
