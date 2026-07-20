using LearningLab.Data.Models.Campaign.Quests;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignQuestRepository;

public sealed class CampaignQuestRepository : ICampaignQuestRepository
{
    private readonly LearningLabContext _context;

    public CampaignQuestRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignQuest>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await QueryQuestsWithTasks()
            .AsNoTracking()
            .Where(quest => quest.CampaignId == campaignId)
            .OrderBy(quest => quest.CompletedAt != null)
            .ThenBy(quest => quest.Type)
            .ThenBy(quest => quest.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignQuest>> ListIncompleteByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await QueryQuestsWithTasks()
            .AsNoTracking()
            .Where(quest => quest.CampaignId == campaignId
                && quest.CompletedAt == null)
            .OrderBy(quest => quest.Type)
            .ThenBy(quest => quest.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignQuest?> GetByCampaignIdAndQuestIdAsync(
        Guid campaignId,
        Guid questId,
        CancellationToken cancellationToken = default)
    {
        return QueryQuestsWithTasks()
            .SingleOrDefaultAsync(
                quest => quest.CampaignId == campaignId
                    && quest.QuestId == questId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignQuest quest,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignQuests.AddAsync(quest, cancellationToken);
    }

    public void Update(CampaignQuest quest)
    {
        _context.CampaignQuests.Update(quest);
    }

    public void Remove(CampaignQuest quest)
    {
        _context.CampaignQuests.Remove(quest);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<CampaignQuest> QueryQuestsWithTasks()
    {
        return _context.CampaignQuests
            .Include(quest => quest.Tasks);
    }
}
