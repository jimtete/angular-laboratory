using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBlockMilestoneRepository;

public sealed class StoryBlockMilestoneRepository : IStoryBlockMilestoneRepository
{
    private readonly LearningLabContext _context;

    public StoryBlockMilestoneRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryBlockMilestone>> ListByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBlockMilestones
            .AsNoTracking()
            .Include(link => link.CampaignMilestone)
            .Where(link => link.StoryBlockId == storyBlockId)
            .OrderBy(link => link.OrderIndex)
            .ThenBy(link => link.CampaignMilestone.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<StoryBlockMilestone?> GetByStoryBlockIdAndCampaignMilestoneIdAsync(
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBlockMilestones
            .Include(link => link.CampaignMilestone)
            .SingleOrDefaultAsync(
                link => link.StoryBlockId == storyBlockId
                    && link.CampaignMilestoneId == campaignMilestoneId,
                cancellationToken);
    }

    public Task<StoryBlockMilestone?> GetByCampaignMilestoneIdAsync(
        int campaignMilestoneId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBlockMilestones
            .AsNoTracking()
            .Include(link => link.CampaignMilestone)
            .SingleOrDefaultAsync(
                link => link.CampaignMilestoneId == campaignMilestoneId,
                cancellationToken);
    }

    public Task<int?> GetLatestOrderIndexByStoryBlockIdAsync(
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBlockMilestones
            .AsNoTracking()
            .Where(link => link.StoryBlockId == storyBlockId)
            .MaxAsync(
                link => (int?)link.OrderIndex,
                cancellationToken);
    }

    public async Task AddAsync(
        StoryBlockMilestone storyBlockMilestone,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBlockMilestones.AddAsync(
            storyBlockMilestone,
            cancellationToken);
    }

    public Task DecrementOrderAfterAsync(
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBlockMilestones
            .Where(link => link.StoryBlockId == storyBlockId
                && link.OrderIndex > orderIndex)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    link => link.OrderIndex,
                    link => link.OrderIndex - 1),
                cancellationToken);
    }

    public void Remove(StoryBlockMilestone storyBlockMilestone)
    {
        _context.StoryBlockMilestones.Remove(storyBlockMilestone);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
