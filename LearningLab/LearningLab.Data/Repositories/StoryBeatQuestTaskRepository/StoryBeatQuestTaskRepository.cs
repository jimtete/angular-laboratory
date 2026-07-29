using LearningLab.Data.Models.Campaign.Quests;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBeatQuestTaskRepository;

public sealed class StoryBeatQuestTaskRepository : IStoryBeatQuestTaskRepository
{
    private readonly LearningLabContext _context;

    public StoryBeatQuestTaskRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryBeatQuestTask>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBeatQuestTasks
            .AsNoTracking()
            .Include(link => link.QuestTask)
            .ThenInclude(task => task.CampaignQuest)
            .Include(link => link.StoryBeat)
            .ThenInclude(beat => beat.StoryBlock)
            .Where(link => link.QuestTask.CampaignQuest.CampaignId == campaignId)
            .OrderBy(link => link.StoryBeat.StoryBlock.OrderIndex)
            .ThenBy(link => link.StoryBeat.OrderIndex)
            .ThenBy(link => link.StoryBeat.SecondaryOrderIndex)
            .ThenBy(link => link.QuestTask.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoryBeatQuestTask>> ListByStoryBeatIdAsync(
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBeatQuestTasks
            .AsNoTracking()
            .Include(link => link.QuestTask)
            .ThenInclude(task => task.CampaignQuest)
            .Include(link => link.StoryBeat)
            .ThenInclude(beat => beat.StoryBlock)
            .Where(link => link.StoryBeatId == storyBeatId)
            .OrderBy(link => link.QuestTask.DateCompleted != null)
            .ThenBy(link => link.QuestTask.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<StoryBeatQuestTask?> GetByCampaignIdAndQuestTaskIdAsync(
        Guid campaignId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeatQuestTasks
            .Include(link => link.QuestTask)
            .ThenInclude(task => task.CampaignQuest)
            .Include(link => link.StoryBeat)
            .ThenInclude(beat => beat.StoryBlock)
            .Where(link => link.QuestTask.CampaignQuest.CampaignId == campaignId
                && link.QuestTaskId == questTaskId)
            .OrderBy(link => link.LinkedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<StoryBeatQuestTask?> GetByStoryBeatIdAndQuestTaskIdAsync(
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeatQuestTasks
            .Include(link => link.QuestTask)
            .ThenInclude(task => task.CampaignQuest)
            .Include(link => link.StoryBeat)
            .ThenInclude(beat => beat.StoryBlock)
            .SingleOrDefaultAsync(
                link => link.StoryBeatId == storyBeatId
                    && link.QuestTaskId == questTaskId,
                cancellationToken);
    }

    public async Task AddAsync(
        StoryBeatQuestTask link,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBeatQuestTasks.AddAsync(link, cancellationToken);
    }

    public void Remove(StoryBeatQuestTask link)
    {
        _context.StoryBeatQuestTasks.Remove(link);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
