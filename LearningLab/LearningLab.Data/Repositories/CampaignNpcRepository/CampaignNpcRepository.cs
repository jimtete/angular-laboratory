using LearningLab.Data.Models.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignNpcRepository;

public sealed class CampaignNpcRepository : ICampaignNpcRepository
{
    private readonly LearningLabContext _context;

    public CampaignNpcRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignNpc>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignNpcs
            .AsNoTracking()
            .Where(npc => npc.CampaignId == campaignId)
            .OrderBy(npc => npc.DisplayName)
            .ThenBy(npc => npc.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignNpc>> ListByCampaignIdAndTagsAsync(
        Guid campaignId,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default)
    {
        if (tags.Count == 0)
        {
            return [];
        }

        return await _context.CampaignNpcs
            .AsNoTracking()
            .Where(npc => npc.CampaignId == campaignId
                && tags.Contains(npc.Tag))
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignNpc?> GetByCampaignIdAndTagAsync(
        Guid campaignId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignNpcs
            .SingleOrDefaultAsync(
                npc => npc.CampaignId == campaignId
                    && npc.Tag == tag,
                cancellationToken);
    }

    public Task<bool> ExistsByCampaignIdAndTagAsync(
        Guid campaignId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignNpcs
            .AnyAsync(
                npc => npc.CampaignId == campaignId
                    && npc.Tag == tag,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignNpc npc,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignNpcs.AddAsync(npc, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
