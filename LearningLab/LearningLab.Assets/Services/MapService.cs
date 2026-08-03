using LearningLab.Assets.Configuration;
using LearningLab.Assets.Models.DTOs.Maps;
using LearningLab.Assets.Repositories.AssetRepository;
using LearningLab.Assets.Repositories.MapRepository;
using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;
using Microsoft.Extensions.Options;
using MapModel = LearningLab.Data.Models.Campaign.Maps.Map;

namespace LearningLab.Assets.Services;

public sealed class MapService : IMapService
{
    private const int MaximumMapNameLength = 256;
    private const int MaximumMapDescriptionLength = 4096;

    private static readonly IReadOnlyDictionary<string, string> SupportedContentTypes = new Dictionary<string, string>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private readonly IAssetRepository _assetRepository;
    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IMapRepository _mapRepository;
    private readonly MapAssetStorageOptions _mapAssetStorageOptions;
    private readonly IUserRepository _userRepository;

    public MapService(
        IAssetRepository assetRepository,
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        ICampaignRepository campaignRepository,
        IMapRepository mapRepository,
        IOptions<MapAssetStorageOptions> mapAssetStorageOptions,
        IUserRepository userRepository)
    {
        _assetRepository = assetRepository;
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _campaignRepository = campaignRepository;
        _mapRepository = mapRepository;
        _mapAssetStorageOptions = mapAssetStorageOptions.Value;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<MapResponse>>> GetCampaignMapsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignReadAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<MapResponse>>(validationStatusCode.Value);
        }

        var maps = await _mapRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<MapResponse>>(
            ApplicationStatusCode.Success,
            maps.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<MapResponse>> CreateCampaignMapAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignMapRequest request,
        byte[]? mapFileBytes,
        string? mapFileContentType,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? string.Empty
            : request.Description.Trim();

        if (string.IsNullOrWhiteSpace(name)
            || name.Length > MaximumMapNameLength
            || description.Length > MaximumMapDescriptionLength
            || !Enum.IsDefined(request.Category)
            || request.ImageWidthPixels < 1
            || request.ImageHeightPixels < 1
            || request.ParentMapId < 1)
        {
            return new ServiceResult<MapResponse>(ApplicationStatusCode.InvalidMap);
        }

        if (mapFileBytes is null || mapFileBytes.Length == 0)
        {
            return new ServiceResult<MapResponse>(ApplicationStatusCode.MapFileRequired);
        }

        if (mapFileBytes.LongLength > _mapAssetStorageOptions.MaxFileSizeBytes)
        {
            return new ServiceResult<MapResponse>(ApplicationStatusCode.MapFileTooLarge);
        }

        if (!IsSupportedMapContentType(mapFileContentType))
        {
            return new ServiceResult<MapResponse>(ApplicationStatusCode.UnsupportedMapFileFormat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<MapResponse>(validationStatusCode.Value);
        }

        MapModel? parentMap = null;

        if (request.ParentMapId is not null)
        {
            parentMap = await _mapRepository.GetByIdAsync(
                request.ParentMapId.Value,
                cancellationToken);

            if (parentMap is null
                || !await _mapRepository.ExistsByIdInCampaignAsync(
                    parentMap.Id,
                    campaignId,
                    cancellationToken))
            {
                return new ServiceResult<MapResponse>(ApplicationStatusCode.MapParentNotFound);
            }

            if (!CanUseParentCategory(request.Category, parentMap.Category))
            {
                return new ServiceResult<MapResponse>(ApplicationStatusCode.InvalidMapParentHierarchy);
            }
        }

        var timestamp = DateTimeOffset.UtcNow;
        var assetUrl = await StoreMapAssetAsync(
            campaignId,
            mapFileBytes,
            mapFileContentType,
            cancellationToken);

        var asset = new Asset
        {
            AssetType = AssetType.Maps,
            Name = name,
            Description = description,
            CampaignIds = [campaignId],
            AssetUrl = assetUrl,
            ContentType = NormalizeContentType(mapFileContentType),
            FileSizeBytes = mapFileBytes.LongLength,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        var map = new MapModel
        {
            ParentMapId = parentMap?.Id,
            Asset = asset,
            Category = request.Category,
            ImageWidthPixels = request.ImageWidthPixels,
            ImageHeightPixels = request.ImageHeightPixels,
            Name = name,
            Description = description,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Campaigns =
            [
                new MapCampaign
                {
                    CampaignId = campaignId,
                    DateAdded = timestamp
                }
            ]
        };

        await _assetRepository.AddAsync(asset, cancellationToken);
        await _mapRepository.AddAsync(map, cancellationToken);
        await _mapRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MapResponse>(
            ApplicationStatusCode.Success,
            ToResponse(map));
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

    private static bool CanUseParentCategory(
        MapCategory childCategory,
        MapCategory parentCategory)
    {
        return parentCategory <= childCategory;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSupportedMapContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
            && SupportedContentTypes.ContainsKey(contentType);
    }

    private static string NormalizeContentType(string? contentType)
    {
        return string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase)
            ? "image/jpeg"
            : contentType ?? string.Empty;
    }

    private async Task<string> StoreMapAssetAsync(
        Guid campaignId,
        byte[] mapFileBytes,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var campaignFolderName = campaignId.ToString("D");
        var fileExtension = SupportedContentTypes[contentType ?? string.Empty];
        var fileName = $"map_{Guid.NewGuid():N}{fileExtension}";
        var mapAssetDirectory = Path.Combine(
            _mapAssetStorageOptions.RootPath,
            "campaigns",
            campaignFolderName,
            "maps");
        var filePath = Path.Combine(mapAssetDirectory, fileName);

        Directory.CreateDirectory(mapAssetDirectory);
        await File.WriteAllBytesAsync(filePath, mapFileBytes, cancellationToken);

        var requestPath = _mapAssetStorageOptions.RequestPath.TrimEnd('/');
        return $"{requestPath}/campaigns/{campaignFolderName}/maps/{fileName}";
    }

    private static MapResponse ToResponse(MapModel map)
    {
        return new MapResponse
        {
            Id = map.Id,
            ParentMapId = map.ParentMapId,
            AssetId = map.AssetId,
            AssetUrl = map.Asset.AssetUrl,
            ContentType = map.Asset.ContentType,
            FileSizeBytes = map.Asset.FileSizeBytes,
            Category = map.Category,
            ImageWidthPixels = map.ImageWidthPixels,
            ImageHeightPixels = map.ImageHeightPixels,
            Name = map.Name,
            Description = map.Description,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt
        };
    }
}
