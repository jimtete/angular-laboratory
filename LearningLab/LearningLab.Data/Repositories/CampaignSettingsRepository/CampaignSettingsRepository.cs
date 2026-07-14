using LearningLab.Data.Models.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignSettingsRepository;

public sealed class CampaignSettingsRepository : ICampaignSettingsRepository
{
    private readonly LearningLabContext _context;

    public CampaignSettingsRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<CampaignSettings?> GetByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignSettings
            .SingleOrDefaultAsync(
                settings => settings.CampaignId == campaignId,
                cancellationToken);
    }

    public Task<bool> ExistsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignSettings
            .AnyAsync(
                settings => settings.CampaignId == campaignId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignSettings campaignSettings,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignSettings.AddAsync(campaignSettings, cancellationToken);
    }

    public void Update(CampaignSettings campaignSettings)
    {
        _context.CampaignSettings.Update(campaignSettings);
    }

    public void Remove(CampaignSettings campaignSettings)
    {
        _context.CampaignSettings.Remove(campaignSettings);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
