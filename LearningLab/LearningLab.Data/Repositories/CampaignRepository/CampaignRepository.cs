using LearningLab.Data.Models.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignRepository;

public sealed class CampaignRepository : ICampaignRepository
{
    private readonly LearningLabContext _context;

    public CampaignRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Campaign>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await QueryCampaignsWithGameMaster()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Campaign>> GetByGameMasterIdAsync(
        Guid gameMasterId,
        CancellationToken cancellationToken = default)
    {
        return await QueryCampaignsWithGameMaster()
            .Where(campaign => campaign.GameMasterId == gameMasterId)
            .ToListAsync(cancellationToken);
    }

    public Task<Campaign?> GetByIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return QueryCampaignsWithGameMaster()
            .SingleOrDefaultAsync(
                campaign => campaign.CampaignId == campaignId,
                cancellationToken);
    }

    public Task<Campaign?> GetByIdForGameMasterAsync(
        Guid campaignId,
        Guid gameMasterId,
        CancellationToken cancellationToken = default)
    {
        return QueryCampaignsWithGameMaster()
            .SingleOrDefaultAsync(
                campaign => campaign.CampaignId == campaignId
                    && campaign.GameMasterId == gameMasterId,
                cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return _context.Campaigns
            .AnyAsync(
                campaign => campaign.CampaignId == campaignId,
                cancellationToken);
    }

    public async Task AddAsync(
        Campaign campaign,
        CancellationToken cancellationToken = default)
    {
        await _context.Campaigns.AddAsync(campaign, cancellationToken);
    }

    public void Update(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
    }

    public void Remove(Campaign campaign)
    {
        _context.Campaigns.Remove(campaign);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Campaign> QueryCampaignsWithGameMaster()
    {
        return _context.Campaigns
            .Include(campaign => campaign.GameMaster);
    }
}
