using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBeatIndexPathRuleRepository;

public sealed class StoryBeatIndexPathRuleRepository : IStoryBeatIndexPathRuleRepository
{
    private readonly LearningLabContext _context;

    public StoryBeatIndexPathRuleRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<StoryBeatIndexPathRule?> GetByCampaignStoryBlockAndOrderIndexAsync(
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBeatIndexPathRules
            .SingleOrDefaultAsync(
                rule => rule.CampaignId == campaignId
                    && rule.StoryBlockId == storyBlockId
                    && rule.OrderIndex == orderIndex,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StoryBeatIndexPathRule>> ListByStoryBlockAsync(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBeatIndexPathRules
            .AsNoTracking()
            .Where(rule => rule.CampaignId == campaignId
                && rule.StoryBlockId == storyBlockId)
            .OrderBy(rule => rule.OrderIndex)
            .ThenBy(rule => rule.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        StoryBeatIndexPathRule rule,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBeatIndexPathRules.AddAsync(rule, cancellationToken);
    }

    public void Remove(StoryBeatIndexPathRule rule)
    {
        _context.StoryBeatIndexPathRules.Remove(rule);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
