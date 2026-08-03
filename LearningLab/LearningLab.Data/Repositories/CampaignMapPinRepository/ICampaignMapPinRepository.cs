using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Models.Campaign.Stores;
using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Repositories.CampaignMapPinRepository;

public interface ICampaignMapPinRepository
{
    Task<Map?> GetMapByCampaignIdAsync(
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MapPin>> ListByMapIdAsync(
        int mapId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MapPinConnection>> ListConnectionsByMapIdAsync(
        int mapId,
        CancellationToken cancellationToken = default);

    Task<MapPin?> GetMutableByMapIdAndPinIdAsync(
        int mapId,
        int pinId,
        CancellationToken cancellationToken = default);

    Task<MapPinConnection?> GetMutableConnectionByMapIdAndConnectionIdAsync(
        int mapId,
        int connectionId,
        CancellationToken cancellationToken = default);

    Task<bool> MapPinsExistByMapIdAsync(
        int mapId,
        IReadOnlyCollection<int> pinIds,
        CancellationToken cancellationToken = default);

    Task<bool> ConnectionExistsAsync(
        int mapId,
        int mapPinAId,
        int mapPinBId,
        int? ignoredConnectionId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TargetExistsAsync(
        Guid campaignId,
        MapPinTargetType targetType,
        string targetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Map>> ListTargetMapsByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> mapIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoryBlock>> ListTargetStoryBlocksByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<Guid> storyBlockIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoreEntry>> ListTargetStoresByIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> storeIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        MapPin pin,
        CancellationToken cancellationToken = default);

    Task AddConnectionAsync(
        MapPinConnection connection,
        CancellationToken cancellationToken = default);

    void Remove(MapPin pin);

    void RemoveConnection(MapPinConnection connection);

    void RemoveConnectionsForPin(MapPin pin);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
