using LearningLab.Data.Models.Campaign.Quests;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignQuestTaskRepository;

public sealed class CampaignQuestTaskRepository : ICampaignQuestTaskRepository
{
    private readonly LearningLabContext _context;

    public CampaignQuestTaskRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignQuestTask>> ListByQuestIdAsync(
        Guid questId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignQuestTasks
            .AsNoTracking()
            .Where(task => task.QuestId == questId)
            .OrderBy(task => task.DateCompleted != null)
            .ThenBy(task => task.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignQuestTask?> GetByQuestIdAndTaskIdAsync(
        Guid questId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignQuestTasks
            .SingleOrDefaultAsync(
                task => task.QuestId == questId
                    && task.QuestTaskId == questTaskId,
                cancellationToken);
    }

    public Task<CampaignQuestTask?> GetByCampaignIdAndTaskIdAsync(
        Guid campaignId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignQuestTasks
            .Include(task => task.CampaignQuest)
            .SingleOrDefaultAsync(
                task => task.CampaignQuest.CampaignId == campaignId
                    && task.QuestTaskId == questTaskId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignQuestTask task,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignQuestTasks.AddAsync(task, cancellationToken);
    }

    public void Update(CampaignQuestTask task)
    {
        _context.CampaignQuestTasks.Update(task);
    }

    public void Remove(CampaignQuestTask task)
    {
        _context.CampaignQuestTasks.Remove(task);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
