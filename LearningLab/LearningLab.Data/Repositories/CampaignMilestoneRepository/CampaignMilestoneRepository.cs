using LearningLab.Data.Models.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignMilestoneRepository;

public sealed class CampaignMilestoneRepository : ICampaignMilestoneRepository
{
    private readonly LearningLabContext _context;

    public CampaignMilestoneRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignMilestone>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignMilestones
            .AsNoTracking()
            .Where(milestone => milestone.CampaignId == campaignId)
            .OrderByDescending(milestone => milestone.AchievedAt ?? milestone.CreatedAt)
            .ThenBy(milestone => milestone.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignMilestone>> ListUnachievedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignMilestones
            .AsNoTracking()
            .Where(milestone => milestone.CampaignId == campaignId
                && milestone.AchievedAt == null)
            .OrderBy(milestone => milestone.Importance)
            .ThenBy(milestone => milestone.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignMilestone>> ListUnlinkedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignMilestones
            .AsNoTracking()
            .Where(milestone => milestone.CampaignId == campaignId
                && milestone.StoryBeat == null)
            .OrderBy(milestone => milestone.Importance)
            .ThenBy(milestone => milestone.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignMilestone?> GetByCampaignIdAndMilestoneIdAsync(
        Guid campaignId,
        int milestoneId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignMilestones
            .SingleOrDefaultAsync(
                milestone => milestone.CampaignId == campaignId
                    && milestone.Id == milestoneId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignMilestone milestone,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignMilestones.AddAsync(milestone, cancellationToken);
    }

    public void Remove(CampaignMilestone milestone)
    {
        _context.CampaignMilestones.Remove(milestone);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
