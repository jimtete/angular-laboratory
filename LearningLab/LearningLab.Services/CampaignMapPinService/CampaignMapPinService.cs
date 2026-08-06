using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Models.Campaign.Stores;
using LearningLab.Data.Models.DTOs.Campaign.Maps;
using LearningLab.Data.Repositories.CampaignMapPinRepository;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignMapPinService;

public sealed class CampaignMapPinService : ICampaignMapPinService
{
    private const int MaximumPinLabelLength = 256;
    private const int MaximumPinDescriptionLength = 4096;
    private const int MaximumTargetIdLength = 128;

    private readonly ICampaignMapPinRepository _campaignMapPinRepository;
    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public CampaignMapPinService(
        ICampaignMapPinRepository campaignMapPinRepository,
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _campaignMapPinRepository = campaignMapPinRepository;
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<MapPinResponse>>> GetMapPinsAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignReadAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<MapPinResponse>>(validationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<IReadOnlyList<MapPinResponse>>(ApplicationStatusCode.MapNotFound);
        }

        var pins = await _campaignMapPinRepository.ListByMapIdAsync(
            mapId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<MapPinResponse>>(
            ApplicationStatusCode.Success,
            pins.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<MapPinsByMapResponse>> GetMapPinsByMapAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignReadAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapPinsByMapResponse>(validationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<MapPinsByMapResponse>(ApplicationStatusCode.MapNotFound);
        }

        var pins = await _campaignMapPinRepository.ListByMapIdAsync(
            mapId,
            cancellationToken);

        var connections = await _campaignMapPinRepository.ListConnectionsByMapIdAsync(
            mapId,
            cancellationToken);

        var detailedPins = await ToDetailedResponsesAsync(
            campaignId,
            pins,
            cancellationToken);

        return new ServiceResult<MapPinsByMapResponse>(
            ApplicationStatusCode.Success,
            new MapPinsByMapResponse
            {
                MapId = mapId,
                PinTypes = GetTargetTypes(),
                Pins = detailedPins,
                Connections = connections.Select(ToConnectionResponse).ToList()
            });
    }

    public ServiceResult<IReadOnlyList<MapPinTargetTypeResponse>> GetMapPinTargetTypes()
    {
        return new ServiceResult<IReadOnlyList<MapPinTargetTypeResponse>>(
            ApplicationStatusCode.Success,
            GetTargetTypes());
    }

    public ServiceResult<IReadOnlyList<MapPinConnectionDistanceUnitResponse>> GetMapPinConnectionDistanceUnits()
    {
        return new ServiceResult<IReadOnlyList<MapPinConnectionDistanceUnitResponse>>(
            ApplicationStatusCode.Success,
            GetConnectionDistanceUnits());
    }

    public async Task<ServiceResult<IReadOnlyList<MapPinConnectionResponse>>> GetMapPinConnectionsAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignReadAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<MapPinConnectionResponse>>(validationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<IReadOnlyList<MapPinConnectionResponse>>(ApplicationStatusCode.MapNotFound);
        }

        var connections = await _campaignMapPinRepository.ListConnectionsByMapIdAsync(
            mapId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<MapPinConnectionResponse>>(
            ApplicationStatusCode.Success,
            connections.Select(ToConnectionResponse).ToList());
    }

    public async Task<ServiceResult<MapPinResponse>> CreateMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CreateMapPinRequest request,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<MapPinResponse>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<MapPinResponse>(ApplicationStatusCode.MapNotFound);
        }

        var validationStatusCode = await ValidatePinRequestAsync(
            campaignId,
            map,
            request.XCoordinate,
            request.YCoordinate,
            request.Label,
            request.Description,
            request.TargetType,
            request.TargetId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapPinResponse>(validationStatusCode.Value);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var pin = new MapPin
        {
            MapId = mapId,
            XCoordinate = request.XCoordinate,
            YCoordinate = request.YCoordinate,
            Label = request.Label!.Trim(),
            Description = NormalizeDescription(request.Description),
            TargetType = request.TargetType,
            TargetId = NormalizeTargetId(request.TargetType, request.TargetId),
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        await _campaignMapPinRepository.AddAsync(pin, cancellationToken);
        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MapPinResponse>(
            ApplicationStatusCode.Success,
            ToResponse(pin));
    }

    public async Task<ServiceResult<MapPinResponse>> UpdateMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int pinId,
        UpdateMapPinRequest request,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<MapPinResponse>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<MapPinResponse>(ApplicationStatusCode.MapNotFound);
        }

        var validationStatusCode = await ValidatePinRequestAsync(
            campaignId,
            map,
            request.XCoordinate,
            request.YCoordinate,
            request.Label,
            request.Description,
            request.TargetType,
            request.TargetId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapPinResponse>(validationStatusCode.Value);
        }

        var pin = await _campaignMapPinRepository.GetMutableByMapIdAndPinIdAsync(
            mapId,
            pinId,
            cancellationToken);

        if (pin is null)
        {
            return new ServiceResult<MapPinResponse>(ApplicationStatusCode.MapPinNotFound);
        }

        pin.XCoordinate = request.XCoordinate;
        pin.YCoordinate = request.YCoordinate;
        pin.Label = request.Label!.Trim();
        pin.Description = NormalizeDescription(request.Description);
        pin.TargetType = request.TargetType;
        pin.TargetId = NormalizeTargetId(request.TargetType, request.TargetId);
        pin.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MapPinResponse>(
            ApplicationStatusCode.Success,
            ToResponse(pin));
    }

    public async Task<ServiceResult<object>> DeleteMapPinAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int pinId,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<object>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.MapNotFound);
        }

        var pin = await _campaignMapPinRepository.GetMutableByMapIdAndPinIdAsync(
            mapId,
            pinId,
            cancellationToken);

        if (pin is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.MapPinNotFound);
        }

        _campaignMapPinRepository.RemoveConnectionsForPin(pin);
        _campaignMapPinRepository.Remove(pin);
        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<MapPinConnectionResponse>> CreateMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        CreateMapPinConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<MapPinConnectionResponse>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<MapPinConnectionResponse>(ApplicationStatusCode.MapNotFound);
        }

        var validationStatusCode = await ValidateConnectionRequestAsync(
            mapId,
            request.MapPinAId,
            request.MapPinBId,
            request.DistanceValue,
            request.DistanceUnit,
            ignoredConnectionId: null,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapPinConnectionResponse>(validationStatusCode.Value);
        }

        var (mapPinAId, mapPinBId) = NormalizeConnectionPins(
            request.MapPinAId,
            request.MapPinBId);
        var timestamp = DateTimeOffset.UtcNow;
        var connection = new MapPinConnection
        {
            MapId = mapId,
            MapPinAId = mapPinAId,
            MapPinBId = mapPinBId,
            DistanceValue = request.DistanceValue,
            DistanceUnit = request.DistanceUnit,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        await _campaignMapPinRepository.AddConnectionAsync(connection, cancellationToken);
        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MapPinConnectionResponse>(
            ApplicationStatusCode.Success,
            ToConnectionResponse(connection));
    }

    public async Task<ServiceResult<MapPinConnectionResponse>> UpdateMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int connectionId,
        UpdateMapPinConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<MapPinConnectionResponse>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<MapPinConnectionResponse>(ApplicationStatusCode.MapNotFound);
        }

        var connection = await _campaignMapPinRepository.GetMutableConnectionByMapIdAndConnectionIdAsync(
            mapId,
            connectionId,
            cancellationToken);

        if (connection is null)
        {
            return new ServiceResult<MapPinConnectionResponse>(ApplicationStatusCode.MapPinConnectionNotFound);
        }

        var validationStatusCode = await ValidateConnectionRequestAsync(
            mapId,
            request.MapPinAId,
            request.MapPinBId,
            request.DistanceValue,
            request.DistanceUnit,
            connectionId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapPinConnectionResponse>(validationStatusCode.Value);
        }

        var (mapPinAId, mapPinBId) = NormalizeConnectionPins(
            request.MapPinAId,
            request.MapPinBId);

        connection.MapPinAId = mapPinAId;
        connection.MapPinBId = mapPinBId;
        connection.DistanceValue = request.DistanceValue;
        connection.DistanceUnit = request.DistanceUnit;
        connection.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MapPinConnectionResponse>(
            ApplicationStatusCode.Success,
            ToConnectionResponse(connection));
    }

    public async Task<ServiceResult<object>> DeleteMapPinConnectionAsync(
        Guid userId,
        Guid campaignId,
        int mapId,
        int connectionId,
        CancellationToken cancellationToken = default)
    {
        var masterValidationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (masterValidationStatusCode is not null)
        {
            return new ServiceResult<object>(masterValidationStatusCode.Value);
        }

        var map = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
            campaignId,
            mapId,
            cancellationToken);

        if (map is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.MapNotFound);
        }

        var connection = await _campaignMapPinRepository.GetMutableConnectionByMapIdAndConnectionIdAsync(
            mapId,
            connectionId,
            cancellationToken);

        if (connection is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.MapPinConnectionNotFound);
        }

        _campaignMapPinRepository.RemoveConnection(connection);
        await _campaignMapPinRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    private async Task<ApplicationStatusCode?> ValidateConnectionRequestAsync(
        int mapId,
        int mapPinAId,
        int mapPinBId,
        decimal? distanceValue,
        MapPinConnectionDistanceUnit? distanceUnit,
        int? ignoredConnectionId,
        CancellationToken cancellationToken)
    {
        if (mapPinAId < 1
            || mapPinBId < 1
            || mapPinAId == mapPinBId
            || (distanceValue is null && distanceUnit is not null)
            || (distanceValue is not null
                && (distanceValue <= 0
                    || distanceUnit is null
                    || !Enum.IsDefined(distanceUnit.Value))))
        {
            return ApplicationStatusCode.InvalidMapPinConnection;
        }

        var (normalizedMapPinAId, normalizedMapPinBId) = NormalizeConnectionPins(
            mapPinAId,
            mapPinBId);

        var pinsExist = await _campaignMapPinRepository.MapPinsExistByMapIdAsync(
            mapId,
            [normalizedMapPinAId, normalizedMapPinBId],
            cancellationToken);

        if (!pinsExist)
        {
            return ApplicationStatusCode.MapPinNotFound;
        }

        var connectionExists = await _campaignMapPinRepository.ConnectionExistsAsync(
            mapId,
            normalizedMapPinAId,
            normalizedMapPinBId,
            ignoredConnectionId,
            cancellationToken);

        return connectionExists
            ? ApplicationStatusCode.MapPinConnectionAlreadyExists
            : null;
    }

    private static (int MapPinAId, int MapPinBId) NormalizeConnectionPins(
        int mapPinAId,
        int mapPinBId)
    {
        return mapPinAId < mapPinBId
            ? (mapPinAId, mapPinBId)
            : (mapPinBId, mapPinAId);
    }

    private async Task<ApplicationStatusCode?> ValidatePinRequestAsync(
        Guid campaignId,
        Map map,
        decimal xCoordinate,
        decimal yCoordinate,
        string? labelValue,
        string? descriptionValue,
        MapPinTargetType targetType,
        string? targetIdValue,
        CancellationToken cancellationToken)
    {
        var label = labelValue?.Trim();
        var description = NormalizeDescription(descriptionValue);
        var targetId = targetIdValue?.Trim();
        var hasMapDimensions = map.ImageWidthPixels > 0 && map.ImageHeightPixels > 0;

        if (string.IsNullOrWhiteSpace(label)
            || label.Length > MaximumPinLabelLength
            || description.Length > MaximumPinDescriptionLength
            || xCoordinate < 0
            || yCoordinate < 0
            || (hasMapDimensions && xCoordinate > map.ImageWidthPixels)
            || (hasMapDimensions && yCoordinate > map.ImageHeightPixels)
            || !Enum.IsDefined(targetType)
            || targetId?.Length > MaximumTargetIdLength)
        {
            return ApplicationStatusCode.InvalidMapPin;
        }

        if (TargetTypeRequiresNoTargetId(targetType))
        {
            return string.IsNullOrWhiteSpace(targetId)
                ? null
                : ApplicationStatusCode.InvalidMapPinTarget;
        }

        if (string.IsNullOrWhiteSpace(targetId))
        {
            return ApplicationStatusCode.InvalidMapPinTarget;
        }

        if (targetType == MapPinTargetType.Map)
        {
            if (!int.TryParse(targetId, out var targetMapId) || targetMapId == map.Id)
            {
                return ApplicationStatusCode.InvalidMapPinTarget;
            }

            var targetMap = await _campaignMapPinRepository.GetMapByCampaignIdAsync(
                campaignId,
                targetMapId,
                cancellationToken);

            if (targetMap is null)
            {
                return ApplicationStatusCode.MapPinTargetNotFound;
            }

            return targetMap.Category >= map.Category
                ? null
                : ApplicationStatusCode.InvalidMapPinTarget;
        }

        var targetExists = await _campaignMapPinRepository.TargetExistsAsync(
            campaignId,
            targetType,
            targetId,
            cancellationToken);

        return targetExists
            ? null
            : ApplicationStatusCode.MapPinTargetNotFound;
    }

    private async Task<ApplicationStatusCode?> ValidateCampaignReadAccessAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return ApplicationStatusCode.UserNotFound;
        }

        if (HasRole(user, AccessRoleNames.Master))
        {
            var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
                campaignId,
                userId,
                cancellationToken);

            if (campaign is not null)
            {
                return null;
            }
        }

        if (HasRole(user, AccessRoleNames.Player))
        {
            if (!await _campaignRepository.ExistsByIdAsync(campaignId, cancellationToken))
            {
                return ApplicationStatusCode.CampaignNotFound;
            }

            var playerJoined = await _campaignParticipationInviteRepository.ExistsParticipationAsync(
                campaignId,
                userId,
                cancellationToken);

            return playerJoined
                ? null
                : ApplicationStatusCode.CampaignNotFound;
        }

        return ApplicationStatusCode.CampaignNotFound;
    }

    private async Task<ApplicationStatusCode?> ValidateMasterCampaignAccessAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return ApplicationStatusCode.UserNotFound;
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return ApplicationStatusCode.CampaignMasterRoleRequired;
        }

        var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
            campaignId,
            userId,
            cancellationToken);

        return campaign is null
            ? ApplicationStatusCode.CampaignNotFound
            : null;
    }

    private static string NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : description.Trim();
    }

    private static string? NormalizeTargetId(
        MapPinTargetType targetType,
        string? targetId)
    {
        return TargetTypeRequiresNoTargetId(targetType)
            ? null
            : targetId?.Trim();
    }

    private static bool TargetTypeRequiresNoTargetId(MapPinTargetType targetType)
    {
        return targetType is MapPinTargetType.Placeholder
            or MapPinTargetType.PlayersPosition;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static MapPinResponse ToResponse(MapPin pin)
    {
        return new MapPinResponse
        {
            Id = pin.Id,
            MapId = pin.MapId,
            XCoordinate = pin.XCoordinate,
            YCoordinate = pin.YCoordinate,
            Label = pin.Label,
            Description = pin.Description,
            TargetType = pin.TargetType,
            TargetId = pin.TargetId,
            CreatedAt = pin.CreatedAt,
            UpdatedAt = pin.UpdatedAt
        };
    }

    private static MapPinConnectionResponse ToConnectionResponse(MapPinConnection connection)
    {
        return new MapPinConnectionResponse
        {
            Id = connection.Id,
            MapId = connection.MapId,
            MapPinAId = connection.MapPinAId,
            MapPinBId = connection.MapPinBId,
            DistanceValue = connection.DistanceValue,
            DistanceUnit = connection.DistanceUnit,
            CreatedAt = connection.CreatedAt,
            UpdatedAt = connection.UpdatedAt
        };
    }

    private static IReadOnlyList<MapPinConnectionDistanceUnitResponse> GetConnectionDistanceUnits()
    {
        return
        [
            new MapPinConnectionDistanceUnitResponse
            {
                Id = (int)MapPinConnectionDistanceUnit.Minutes,
                Unit = MapPinConnectionDistanceUnit.Minutes,
                Name = "Minutes"
            },
            new MapPinConnectionDistanceUnitResponse
            {
                Id = (int)MapPinConnectionDistanceUnit.Hours,
                Unit = MapPinConnectionDistanceUnit.Hours,
                Name = "Hours"
            },
            new MapPinConnectionDistanceUnitResponse
            {
                Id = (int)MapPinConnectionDistanceUnit.Days,
                Unit = MapPinConnectionDistanceUnit.Days,
                Name = "Days"
            },
            new MapPinConnectionDistanceUnitResponse
            {
                Id = (int)MapPinConnectionDistanceUnit.Weeks,
                Unit = MapPinConnectionDistanceUnit.Weeks,
                Name = "Weeks"
            }
        ];
    }

    private async Task<IReadOnlyList<MapPinDetailsResponse>> ToDetailedResponsesAsync(
        Guid campaignId,
        IReadOnlyList<MapPin> pins,
        CancellationToken cancellationToken)
    {
        var mapIds = pins
            .Where(pin => pin.TargetType == MapPinTargetType.Map)
            .Select(pin => int.TryParse(pin.TargetId, out var mapId) ? mapId : (int?)null)
            .Where(mapId => mapId.HasValue)
            .Select(mapId => mapId!.Value)
            .Distinct()
            .ToList();

        var storyBlockIds = pins
            .Where(pin => pin.TargetType == MapPinTargetType.StoryBlock)
            .Select(pin => Guid.TryParse(pin.TargetId, out var storyBlockId) ? storyBlockId : (Guid?)null)
            .Where(storyBlockId => storyBlockId.HasValue)
            .Select(storyBlockId => storyBlockId!.Value)
            .Distinct()
            .ToList();

        var storeIds = pins
            .Where(pin => pin.TargetType == MapPinTargetType.Store)
            .Select(pin => int.TryParse(pin.TargetId, out var storeId) ? storeId : (int?)null)
            .Where(storeId => storeId.HasValue)
            .Select(storeId => storeId!.Value)
            .Distinct()
            .ToList();

        var targetMaps = await _campaignMapPinRepository.ListTargetMapsByIdsAsync(
            campaignId,
            mapIds,
            cancellationToken);

        var targetStoryBlocks = await _campaignMapPinRepository.ListTargetStoryBlocksByIdsAsync(
            campaignId,
            storyBlockIds,
            cancellationToken);

        var targetStores = await _campaignMapPinRepository.ListTargetStoresByIdsAsync(
            campaignId,
            storeIds,
            cancellationToken);

        var targetMapsById = targetMaps.ToDictionary(map => map.Id);
        var targetStoryBlocksById = targetStoryBlocks.ToDictionary(storyBlock => storyBlock.StoryBlockId);
        var targetStoresById = targetStores.ToDictionary(store => store.StoreId);

        return pins
            .Select(pin => ToDetailsResponse(
                pin,
                ResolveTargetData(
                    pin,
                    targetMapsById,
                    targetStoryBlocksById,
                    targetStoresById)))
            .ToList();
    }

    private static MapPinDetailsResponse ToDetailsResponse(
        MapPin pin,
        MapPinTargetDataResponse? targetData)
    {
        return new MapPinDetailsResponse
        {
            Id = pin.Id,
            MapId = pin.MapId,
            XCoordinate = pin.XCoordinate,
            YCoordinate = pin.YCoordinate,
            Label = pin.Label,
            Description = pin.Description,
            TargetType = pin.TargetType,
            TargetId = pin.TargetId,
            TargetData = targetData,
            CreatedAt = pin.CreatedAt,
            UpdatedAt = pin.UpdatedAt
        };
    }

    private static MapPinTargetDataResponse? ResolveTargetData(
        MapPin pin,
        IReadOnlyDictionary<int, Map> targetMapsById,
        IReadOnlyDictionary<Guid, Data.Models.Campaign.Story.StoryBlock> targetStoryBlocksById,
        IReadOnlyDictionary<int, StoreEntry> targetStoresById)
    {
        return pin.TargetType switch
        {
            MapPinTargetType.Placeholder => null,
            MapPinTargetType.PlayersPosition => null,
            MapPinTargetType.StoryBlock => Guid.TryParse(pin.TargetId, out var storyBlockId)
                && targetStoryBlocksById.TryGetValue(storyBlockId, out var storyBlock)
                    ? new MapPinTargetDataResponse
                    {
                        TargetType = pin.TargetType,
                        TargetId = pin.TargetId,
                        Name = storyBlock.Title,
                        StoryBlockId = storyBlock.StoryBlockId,
                        StoryBlockOrderIndex = storyBlock.OrderIndex
                    }
                    : null,
            MapPinTargetType.Map => int.TryParse(pin.TargetId, out var mapId)
                && targetMapsById.TryGetValue(mapId, out var map)
                    ? new MapPinTargetDataResponse
                    {
                        TargetType = pin.TargetType,
                        TargetId = pin.TargetId,
                        Name = map.Name,
                        Description = map.Description,
                        MapId = map.Id,
                        ParentMapId = map.ParentMapId,
                        AssetId = map.AssetId,
                        AssetUrl = map.Asset.AssetUrl,
                        ContentType = map.Asset.ContentType,
                        MapCategory = map.Category,
                        ImageWidthPixels = map.ImageWidthPixels,
                        ImageHeightPixels = map.ImageHeightPixels
                    }
                    : null,
            MapPinTargetType.Store => int.TryParse(pin.TargetId, out var storeId)
                && targetStoresById.TryGetValue(storeId, out var store)
                    ? new MapPinTargetDataResponse
                    {
                        TargetType = pin.TargetType,
                        TargetId = pin.TargetId,
                        Name = store.StoreName ?? store.StoreLocation,
                        Description = store.StoreDescription,
                        StoreId = store.StoreId,
                        StoreType = store.StoreType,
                        StoreLockState = store.LockState,
                        StoreLocation = store.StoreLocation
                    }
                    : null,
            _ => null
        };
    }

    private static IReadOnlyList<MapPinTargetTypeResponse> GetTargetTypes()
    {
        return
        [
            new MapPinTargetTypeResponse
            {
                Id = (int)MapPinTargetType.Placeholder,
                Type = MapPinTargetType.Placeholder,
                Name = "Placeholder",
                RequiresTargetId = false
            },
            new MapPinTargetTypeResponse
            {
                Id = (int)MapPinTargetType.StoryBlock,
                Type = MapPinTargetType.StoryBlock,
                Name = "Story Block",
                RequiresTargetId = true
            },
            new MapPinTargetTypeResponse
            {
                Id = (int)MapPinTargetType.Map,
                Type = MapPinTargetType.Map,
                Name = "Another Map",
                RequiresTargetId = true
            },
            new MapPinTargetTypeResponse
            {
                Id = (int)MapPinTargetType.Store,
                Type = MapPinTargetType.Store,
                Name = "Store",
                RequiresTargetId = true
            },
            new MapPinTargetTypeResponse
            {
                Id = (int)MapPinTargetType.PlayersPosition,
                Type = MapPinTargetType.PlayersPosition,
                Name = "Players Position",
                RequiresTargetId = false
            }
        ];
    }
}
