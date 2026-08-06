using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBlockMusicFileRepository;

public interface IStoryBlockMusicFileRepository
{
    Task<IReadOnlyList<StoryBlockMusicFile>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoryBlockMusicFile>> ListByStoryBlockIdsAsync(
        IReadOnlyCollection<Guid> storyBlockIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoryBlockMusicFile>> ListTrackedByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ListExistingMusicFileIdsByUserIdAsync(
        Guid userId,
        IReadOnlyCollection<int> musicFileIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ListExistingStoryBeatIdsByStoryBlockIdAsync(
        Guid storyBlockId,
        IReadOnlyCollection<Guid> storyBeatIds,
        CancellationToken cancellationToken = default);

    Task<int> CountMusicFilesByUserIdAndMusicFileIdsAsync(
        Guid userId,
        IReadOnlyCollection<int> musicFileIds,
        CancellationToken cancellationToken = default);

    Task<int> CountStoryBeatsByStoryBlockIdAndStoryBeatIdsAsync(
        Guid storyBlockId,
        IReadOnlyCollection<Guid> storyBeatIds,
        CancellationToken cancellationToken = default);

    Task RemoveByStoryBeatIdAsync(
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IReadOnlyCollection<StoryBlockMusicFile> links,
        CancellationToken cancellationToken = default);

    Task ReplaceByStoryBlockIdAsync(
        Guid storyBlockId,
        IReadOnlyCollection<StoryBlockMusicFile> links,
        CancellationToken cancellationToken = default);

    void RemoveRange(IReadOnlyCollection<StoryBlockMusicFile> links);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
