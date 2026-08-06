using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Models.Campaign.Stores;
using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignMapPinRepository;

public sealed class CampaignMapPinRepository : ICampaignMapPinRepository
{
    private readonly LearningLabContext _context;

    public CampaignMapPinRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<Map?> GetMapByCampaignIdAsync(
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default)
    {
        return _context.Maps
            .AsNoTracking()
            .SingleOrDefaultAsync(
                map => map.Id == mapId
                    && map.Campaigns.Any(mapCampaign => mapCampaign.CampaignId == campaignId),
                cancellationToken);
    }

    public async Task<IReadOnlyList<MapPin>> ListByMapIdAsync(
        int mapId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MapPins
            .AsNoTracking()
            .Where(pin => pin.MapId == mapId)
            .OrderBy(pin => pin.Label)
            .ThenBy(pin => pin.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MapPinConnection>> ListConnectionsByMapIdAsync(
        int mapId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MapPinConnections
            .AsNoTracking()
            .Where(connection => connection.MapId == mapId)
            .OrderBy(connection => connection.MapPinAId)
            .ThenBy(connection => connection.MapPinBId)
            .ThenBy(connection => connection.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MapPin>> ListStoryBlockPinsByCampaignIdAsync(
        Guid campaignId,
        IReadOnlyCollection<Guid> storyBlockIds,
        CancellationToken cancellationToken = default)
    {
        if (storyBlockIds.Count == 0)
        {
            return [];
        }

        var targetIds = storyBlockIds
            .Select(storyBlockId => storyBlockId.ToString())
            .ToList();

        return await _context.MapPins
            .AsNoTracking()
            .Where(pin => pin.TargetType == MapPinTargetType.StoryBlock
                && pin.TargetId != null
                && targetIds.Contains(pin.TargetId)
                && pin.Map.Campaigns.Any(mapCampaign => mapCampaign.CampaignId == campaignId))
            .OrderBy(pin => pin.MapId)
            .ThenBy(pin => pin.Label)
            .ThenBy(pin => pin.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<MapPin?> GetMutableByMapIdAndPinIdAsync(
        int mapId,
        int pinId,
        CancellationToken cancellationToken = default)
    {
        return _context.MapPins
            .SingleOrDefaultAsync(
                pin => pin.MapId == mapId
                    && pin.Id == pinId,
                cancellationToken);
    }

    public Task<MapPinConnection?> GetMutableConnectionByMapIdAndConnectionIdAsync(
        int mapId,
        int connectionId,
        CancellationToken cancellationToken = default)
    {
        return _context.MapPinConnections
            .SingleOrDefaultAsync(
                connection => connection.MapId == mapId
                    && connection.Id == connectionId,
                cancellationToken);
    }

    public async Task<bool> MapPinsExistByMapIdAsync(
        int mapId,
        IReadOnlyCollection<int> pinIds,
        CancellationToken cancellationToken = default)
    {
        var distinctPinIds = pinIds.Distinct().ToList();

        if (distinctPinIds.Count == 0)
        {
            return false;
        }

        var existingPinCount = await _context.MapPins
            .AsNoTracking()
            .CountAsync(
                pin => pin.MapId == mapId
                    && distinctPinIds.Contains(pin.Id),
                cancellationToken);

        return existingPinCount == distinctPinIds.Count;
    }

    public Task<bool> ConnectionExistsAsync(
        int mapId,
        int mapPinAId,
        int mapPinBId,
        int? ignoredConnectionId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.MapPinConnections
            .AsNoTracking()
            .AnyAsync(
                connection => connection.MapId == mapId
                    && connection.MapPinAId == mapPinAId
                    && connection.MapPinBId == mapPinBId
                    && (ignoredConnectionId == null || connection.Id != ignoredConnectionId.Value),
                cancellationToken);
    }

    public Task<bool> TargetExistsAsync(
        Guid campaignId,
        MapPinTargetType targetType,
        string targetId,
        CancellationToken cancellationToken = default)
    {
        return targetType switch
        {
            MapPinTargetType.StoryBlock => Guid.TryParse(targetId, out var storyBlockId)
                ? _context.StoryBlocks.AnyAsync(
                    storyBlock => storyBlock.CampaignId == campaignId
                        && storyBlock.StoryBlockId == storyBlockId,
                    cancellationToken)
                : Task.FromResult(false),
            MapPinTargetType.Map => int.TryParse(targetId, out var mapId)
                ? _context.MapCampaigns.AnyAsync(
                    mapCampaign => mapCampaign.CampaignId == campaignId
                        && mapCampaign.MapId == mapId,
                    cancellationToken)
                : Task.FromResult(false),
            MapPinTargetType.Store => int.TryParse(targetId, out var storeId)
                ? _context.StoreEntries.AnyAsync(
                    store => store.CampaignId == campaignId
                        && store.StoreId == storeId,
                    cancellationToken)
                : Task.FromResult(false),
            _ => Task.FromResult(false)
        };
    }

    public async Task<IReadOnlyList<Map>> ListTargetMapsByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> mapIds,
        CancellationToken cancellationToken = default)
    {
        if (mapIds.Count == 0)
        {
            return [];
        }

        return await _context.Maps
            .AsNoTracking()
            .Include(map => map.Asset)
            .Where(map => mapIds.Contains(map.Id)
                && map.Campaigns.Any(mapCampaign => mapCampaign.CampaignId == campaignId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoryBlock>> ListTargetStoryBlocksByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<Guid> storyBlockIds,
        CancellationToken cancellationToken = default)
    {
        if (storyBlockIds.Count == 0)
        {
            return [];
        }

        return await _context.StoryBlocks
            .AsNoTracking()
            .Where(storyBlock => storyBlock.CampaignId == campaignId
                && storyBlockIds.Contains(storyBlock.StoryBlockId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoreEntry>> ListTargetStoresByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> storeIds,
        CancellationToken cancellationToken = default)
    {
        if (storeIds.Count == 0)
        {
            return [];
        }

        return await _context.StoreEntries
            .AsNoTracking()
            .Where(store => store.CampaignId == campaignId
                && storeIds.Contains(store.StoreId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        MapPin pin,
        CancellationToken cancellationToken = default)
    {
        await _context.MapPins.AddAsync(pin, cancellationToken);
    }

    public async Task AddConnectionAsync(
        MapPinConnection connection,
        CancellationToken cancellationToken = default)
    {
        await _context.MapPinConnections.AddAsync(connection, cancellationToken);
    }

    public void Remove(MapPin pin)
    {
        _context.MapPins.Remove(pin);
    }

    public void RemoveConnection(MapPinConnection connection)
    {
        _context.MapPinConnections.Remove(connection);
    }

    public void RemoveConnectionsForPin(MapPin pin)
    {
        _context.MapPinConnections.RemoveRange(
            _context.MapPinConnections.Where(connection =>
                connection.MapId == pin.MapId
                && (connection.MapPinAId == pin.Id || connection.MapPinBId == pin.Id)));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
