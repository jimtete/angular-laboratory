using LearningLab.Data;
using LearningLab.Data.Models.Campaign.Maps;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Assets.Repositories.MapRepository;

public sealed class MapRepository : IMapRepository
{
    private readonly LearningLabContext _context;

    public MapRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Map>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Maps
            .AsNoTracking()
            .Include(map => map.Asset)
            .Where(map => map.Campaigns.Any(mapCampaign => mapCampaign.CampaignId == campaignId))
            .OrderBy(map => map.Category)
            .ThenBy(map => map.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Map?> GetByIdAsync(
        int mapId,
        CancellationToken cancellationToken = default)
    {
        return _context.Maps
            .AsNoTracking()
            .Include(map => map.Asset)
            .SingleOrDefaultAsync(
                map => map.Id == mapId,
                cancellationToken);
    }

    public Task<bool> ExistsByIdInCampaignAsync(
        int mapId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return _context.MapCampaigns
            .AsNoTracking()
            .AnyAsync(
                mapCampaign => mapCampaign.MapId == mapId
                    && mapCampaign.CampaignId == campaignId,
                cancellationToken);
    }

    public async Task AddAsync(
        Map map,
        CancellationToken cancellationToken = default)
    {
        await _context.Maps.AddAsync(map, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
