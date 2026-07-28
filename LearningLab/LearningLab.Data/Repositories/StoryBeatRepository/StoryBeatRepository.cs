using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBeatRepository;

public sealed class StoryBeatRepository : IStoryBeatRepository
{
    private readonly LearningLabContext _context;

    public StoryBeatRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryBeat>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBeats
            .AsNoTracking()
            .Include(beat => beat.Milestone)
            .Where(beat => beat.StoryBlockId == storyBlockId)
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<StoryBeat?> GetByStoryBlockIdAndStoryBeatIdAsync(
        Guid storyBlockId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .Include(beat => beat.Milestone)
            .SingleOrDefaultAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && beat.Id == storyBeatId,
                cancellationToken);
    }

    public Task<StoryBeat?> GetFirstByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .Where(beat => beat.StoryBlockId == storyBlockId)
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<StoryBeat?> GetByCampaignIdAndStoryBeatIdAsync(
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .Include(beat => beat.StoryBlock)
            .Include(beat => beat.Milestone)
            .SingleOrDefaultAsync(
                beat => beat.StoryBlock.CampaignId == campaignId
                    && beat.Id == storyBeatId,
                cancellationToken);
    }

    public Task<StoryBeat?> GetByCampaignIdAndCampaignMilestoneIdAsync(
        Guid campaignId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .Include(beat => beat.StoryBlock)
            .Include(beat => beat.Milestone)
            .SingleOrDefaultAsync(
                beat => beat.StoryBlock.CampaignId == campaignId
                    && beat.CampaignMilestoneId == campaignMilestoneId,
                cancellationToken);
    }

    public Task<int?> GetLatestOrderIndexByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .Where(beat => beat.StoryBlockId == storyBlockId)
            .MaxAsync(
                beat => (int?)beat.OrderIndex,
                cancellationToken);
    }

    public Task<int?> GetLatestSecondaryOrderIndexByStoryBlockIdAndOrderIndexAsync(
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .Where(beat => beat.StoryBlockId == storyBlockId
                && beat.OrderIndex == orderIndex)
            .MaxAsync(
                beat => (int?)beat.SecondaryOrderIndex,
                cancellationToken);
    }

    public Task<bool> OrderExistsAsync(
        Guid storyBlockId,
        int orderIndex,
        int secondaryOrderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .AnyAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && beat.OrderIndex == orderIndex
                    && beat.SecondaryOrderIndex == secondaryOrderIndex,
                cancellationToken);
    }

    public Task<bool> TransitionExistsByStoryBlockIdAsync(
        Guid storyBlockId,
        Guid? excludedStoryBeatId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .AnyAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && beat.StoryBeatType == StoryBeatType.Transition
                    && (excludedStoryBeatId == null || beat.Id != excludedStoryBeatId),
                cancellationToken);
    }

    public Task<bool> HasStoryBeatAfterAsync(
        Guid storyBlockId,
        int orderIndex,
        int secondaryOrderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .AnyAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && (beat.OrderIndex > orderIndex
                        || (beat.OrderIndex == orderIndex
                            && beat.SecondaryOrderIndex > secondaryOrderIndex)),
                cancellationToken);
    }

    public async Task AddAsync(
        StoryBeat storyBeat,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBeats.AddAsync(storyBeat, cancellationToken);
    }

    public async Task DecrementOrderAfterAsync(
        Guid storyBlockId,
        int orderIndex,
        int secondaryOrderIndex,
        CancellationToken cancellationToken = default)
    {
        var primaryOrderExists = await _context.StoryBeats
            .AsNoTracking()
            .AnyAsync(
                beat => beat.StoryBlockId == storyBlockId
                    && beat.OrderIndex == orderIndex,
                cancellationToken);

        if (primaryOrderExists)
        {
            await _context.StoryBeats
                .Where(beat => beat.StoryBlockId == storyBlockId
                    && beat.OrderIndex == orderIndex
                    && beat.SecondaryOrderIndex > secondaryOrderIndex)
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(
                        beat => beat.SecondaryOrderIndex,
                        beat => beat.SecondaryOrderIndex - 1),
                    cancellationToken);

            return;
        }

        await _context.StoryBeats
            .Where(beat => beat.StoryBlockId == storyBlockId
                && beat.OrderIndex > orderIndex)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    beat => beat.OrderIndex,
                    beat => beat.OrderIndex - 1),
                cancellationToken);
    }

    public void Update(StoryBeat storyBeat)
    {
        _context.StoryBeats.Update(storyBeat);
    }

    public void Remove(StoryBeat storyBeat)
    {
        _context.StoryBeats.Remove(storyBeat);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
