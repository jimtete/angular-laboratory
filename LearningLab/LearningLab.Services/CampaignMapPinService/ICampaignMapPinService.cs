using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Maps;

namespace LearningLab.Services.CampaignMapPinService;

public interface ICampaignMapPinService
{
    Task<ServiceResult<IReadOnlyList<MapPinResponse>>> GetMapPinsAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapPinsByMapResponse>> GetMapPinsByMapAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default);

    ServiceResult<IReadOnlyList<MapPinTargetTypeResponse>> GetMapPinTargetTypes();

    ServiceResult<IReadOnlyList<MapPinConnectionDistanceUnitResponse>> GetMapPinConnectionDistanceUnits();

    Task<ServiceResult<IReadOnlyList<MapPinConnectionResponse>>> GetMapPinConnectionsAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapPinResponse>> CreateMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CreateMapPinRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapPinResponse>> UpdateMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int pinId,
        UpdateMapPinRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int pinId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapPinConnectionResponse>> CreateMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CreateMapPinConnectionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapPinConnectionResponse>> UpdateMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int connectionId,
        UpdateMapPinConnectionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int connectionId,
        CancellationToken cancellationToken = default);
}
