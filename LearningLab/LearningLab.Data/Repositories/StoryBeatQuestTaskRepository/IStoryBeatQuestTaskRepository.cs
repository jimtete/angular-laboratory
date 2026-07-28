using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Repositories.StoryBeatQuestTaskRepository;

public interface IStoryBeatQuestTaskRepository
{
    Task<IReadOnlyList<StoryBeatQuestTask>> ListByStoryBeatIdAsync(
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<StoryBeatQuestTask?> GetByStoryBeatIdAndQuestTaskIdAsync(
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StoryBeatQuestTask link,
        CancellationToken cancellationToken = default);

    void Remove(StoryBeatQuestTask link);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
