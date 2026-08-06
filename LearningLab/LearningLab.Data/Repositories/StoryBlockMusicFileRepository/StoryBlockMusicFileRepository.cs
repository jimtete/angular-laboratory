using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBlockMusicFileRepository;

public sealed class StoryBlockMusicFileRepository : IStoryBlockMusicFileRepository
{
    private readonly LearningLabContext _context;

    public StoryBlockMusicFileRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryBlockMusicFile>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBlockMusicFiles
            .AsNoTracking()
            .Include(link => link.MusicFile)
            .Where(link => link.StoryBlockId == storyBlockId)
            .OrderBy(link => link.OrderIndex)
            .ThenBy(link => link.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoryBlockMusicFile>> ListByStoryBlockIdsAsync(
        IReadOnlyCollection<Guid> storyBlockIds,
        CancellationToken cancellationToken = default)
    {
        if (storyBlockIds.Count == 0)
        {
            return [];
        }

        return await _context.StoryBlockMusicFiles
            .AsNoTracking()
            .Include(link => link.MusicFile)
            .Where(link => storyBlockIds.Contains(link.StoryBlockId))
            .OrderBy(link => link.StoryBlockId)
            .ThenBy(link => link.OrderIndex)
            .ThenBy(link => link.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoryBlockMusicFile>> ListTrackedByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBlockMusicFiles
            .Where(link => link.StoryBlockId == storyBlockId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> ListExistingMusicFileIdsByUserIdAsync(
        Guid userId,
        IReadOnlyCollection<int> musicFileIds,
        CancellationToken cancellationToken = default)
    {
        if (musicFileIds.Count == 0)
        {
            return [];
        }

        return await _context.MusicFiles
            .AsNoTracking()
            .Where(file => file.UploadedByUserId == userId
                && musicFileIds.Contains(file.Id))
            .Select(file => file.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListExistingStoryBeatIdsByStoryBlockIdAsync(
        Guid storyBlockId,
        IReadOnlyCollection<Guid> storyBeatIds,
        CancellationToken cancellationToken = default)
    {
        if (storyBeatIds.Count == 0)
        {
            return [];
        }

        return await _context.StoryBeats
            .AsNoTracking()
            .Where(beat => beat.StoryBlockId == storyBlockId
                && storyBeatIds.Contains(beat.Id))
            .Select(beat => beat.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountMusicFilesByUserIdAndMusicFileIdsAsync(
        Guid userId,
        IReadOnlyCollection<int> musicFileIds,
        CancellationToken cancellationToken = default)
    {
        if (musicFileIds.Count == 0)
        {
            return Task.FromResult(0);
        }

        return _context.MusicFiles
            .AsNoTracking()
            .CountAsync(
                file => file.UploadedByUserId == userId
                    && musicFileIds.Contains(file.Id),
                cancellationToken);
    }

    public Task<int> CountStoryBeatsByStoryBlockIdAndStoryBeatIdsAsync(
        Guid storyBlockId,
        IReadOnlyCollection<Guid> storyBeatIds,
        CancellationToken cancellationToken = default)
    {
        if (storyBeatIds.Count == 0)
        {
            return Task.FromResult(0);
        }

        return _context.StoryBeats
            .AsNoTracking()
            .CountAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && storyBeatIds.Contains(beat.Id),
                cancellationToken);
    }

    public async Task RemoveByStoryBeatIdAsync(
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBlockMusicFiles
            .Where(link => link.StoryBeatId == storyBeatId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IReadOnlyCollection<StoryBlockMusicFile> links,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBlockMusicFiles.AddRangeAsync(links, cancellationToken);
    }

    public async Task ReplaceByStoryBlockIdAsync(
        Guid storyBlockId,
        IReadOnlyCollection<StoryBlockMusicFile> links,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBlockMusicFiles
            .Where(link => link.StoryBlockId == storyBlockId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.StoryBlockMusicFiles.AddRangeAsync(links, cancellationToken);
    }

    public void RemoveRange(IReadOnlyCollection<StoryBlockMusicFile> links)
    {
        _context.StoryBlockMusicFiles.RemoveRange(links);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
