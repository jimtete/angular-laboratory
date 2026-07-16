using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Models.DTOs.Assets;
using LearningLab.Data.Repositories.AssetQueryRepository;
using LearningLab.Data.Repositories.AssetRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.AssetService;

public sealed class AssetService : IAssetService
{
    private const int MaximumAssetNameLength = 256;
    private const int MaximumAssetDescriptionLength = 4096;

    private readonly IAssetQueryRepository _assetQueryRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public AssetService(
        IAssetQueryRepository assetQueryRepository,
        IAssetRepository assetRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _assetQueryRepository = assetQueryRepository;
        _assetRepository = assetRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<AssetResponse>>> GetAvailableItemsByCampaignIdAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<AssetResponse>>(
                validationStatusCode.Value);
        }

        var items = await _assetQueryRepository.GetAvailableItemsByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<AssetResponse>>(
            ApplicationStatusCode.Success,
            items);
    }

    public async Task<ServiceResult<IReadOnlyList<AssetResponse>>> GetAssetsAsync(
        int? parentAssetId,
        CancellationToken cancellationToken = default)
    {
        var parentValidationStatusCode = await ValidateParentAssetAsync(
            parentAssetId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<AssetResponse>>(
                parentValidationStatusCode.Value);
        }

        var assets = await _assetRepository.ListByParentAssetIdAsync(
            parentAssetId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<AssetResponse>>(
            ApplicationStatusCode.Success,
            assets.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<AssetResponse>> CreateFolderAsync(
        CreateAssetFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildAsset(
            request?.ParentAssetId,
            request?.Name,
            request?.Description,
            AssetType.Folder,
            null,
            null,
            DateTimeOffset.UtcNow,
            out var asset))
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.InvalidAsset);
        }

        var parentValidationStatusCode = await ValidateParentAssetAsync(
            asset.ParentAssetId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<AssetResponse>(parentValidationStatusCode.Value);
        }

        var alreadyExists = await _assetRepository.ExistsByParentAssetIdAndNameAsync(
            asset.ParentAssetId,
            asset.Name,
            cancellationToken);

        if (alreadyExists)
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.AssetAlreadyExists);
        }

        await _assetRepository.AddAsync(asset, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<AssetResponse>(
            ApplicationStatusCode.Success,
            ToResponse(asset));
    }

    public async Task<ServiceResult<AssetResponse>> CreateItemAsync(
        CreateItemAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || !Enum.IsDefined(request.ItemType)
            || !TryBuildAsset(
                request.ParentAssetId,
                request.Name,
                request.Description,
                AssetType.Items,
                request.ItemType,
                request.CampaignIds,
                DateTimeOffset.UtcNow,
                out var asset))
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.InvalidAsset);
        }

        var parentValidationStatusCode = await ValidateParentAssetAsync(
            asset.ParentAssetId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<AssetResponse>(parentValidationStatusCode.Value);
        }

        var campaignValidationStatusCode = await ValidateCampaignIdsAsync(
            asset.CampaignIds,
            cancellationToken);

        if (campaignValidationStatusCode is not null)
        {
            return new ServiceResult<AssetResponse>(campaignValidationStatusCode.Value);
        }

        var alreadyExists = await _assetRepository.ExistsByParentAssetIdAndNameAsync(
            asset.ParentAssetId,
            asset.Name,
            cancellationToken);

        if (alreadyExists)
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.AssetAlreadyExists);
        }

        await _assetRepository.AddAsync(asset, cancellationToken);
        await _assetRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<AssetResponse>(
            ApplicationStatusCode.Success,
            ToResponse(asset));
    }

    public async Task<ServiceResult<AssetResponse>> UpdateItemAsync(
        int assetId,
        UpdateItemAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (assetId < 1
            || request is null
            || !Enum.IsDefined(request.ItemType)
            || !TryBuildAsset(
                request.ParentAssetId,
                request.Name,
                request.Description,
                AssetType.Items,
                request.ItemType,
                request.CampaignIds,
                DateTimeOffset.UtcNow,
                out var updatedAsset))
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.InvalidAsset);
        }

        var asset = await _assetRepository.GetMutableByIdAsync(
            assetId,
            cancellationToken);

        if (asset is null)
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.AssetNotFound);
        }

        if (asset.AssetType != AssetType.Items)
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.InvalidAsset);
        }

        var parentValidationStatusCode = await ValidateParentAssetAsync(
            updatedAsset.ParentAssetId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<AssetResponse>(parentValidationStatusCode.Value);
        }

        var campaignValidationStatusCode = await ValidateCampaignIdsAsync(
            updatedAsset.CampaignIds,
            cancellationToken);

        if (campaignValidationStatusCode is not null)
        {
            return new ServiceResult<AssetResponse>(campaignValidationStatusCode.Value);
        }

        var alreadyExists = await _assetRepository.ExistsByParentAssetIdAndNameExcludingAssetIdAsync(
            updatedAsset.ParentAssetId,
            updatedAsset.Name,
            assetId,
            cancellationToken);

        if (alreadyExists)
        {
            return new ServiceResult<AssetResponse>(ApplicationStatusCode.AssetAlreadyExists);
        }

        asset.ParentAssetId = updatedAsset.ParentAssetId;
        asset.Name = updatedAsset.Name;
        asset.Description = updatedAsset.Description;
        asset.ItemType = updatedAsset.ItemType;
        asset.CampaignIds = updatedAsset.CampaignIds;
        asset.UpdatedAt = DateTimeOffset.UtcNow;

        await _assetRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<AssetResponse>(
            ApplicationStatusCode.Success,
            ToResponse(asset));
    }

    private async Task<ApplicationStatusCode?> ValidateParentAssetAsync(
        int? parentAssetId,
        CancellationToken cancellationToken)
    {
        if (parentAssetId is null)
        {
            return null;
        }

        if (parentAssetId < 1)
        {
            return ApplicationStatusCode.InvalidAsset;
        }

        var parentAsset = await _assetRepository.GetByIdAsync(
            parentAssetId.Value,
            cancellationToken);

        if (parentAsset is null)
        {
            return ApplicationStatusCode.AssetParentNotFound;
        }

        return parentAsset.AssetType == AssetType.Folder
            ? null
            : ApplicationStatusCode.AssetParentMustBeFolder;
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

    private async Task<ApplicationStatusCode?> ValidateCampaignIdsAsync(
        IReadOnlyCollection<Guid>? campaignIds,
        CancellationToken cancellationToken)
    {
        if (campaignIds is null || campaignIds.Count == 0)
        {
            return null;
        }

        var existingCampaignCount = await _campaignRepository.CountByIdsAsync(
            campaignIds,
            cancellationToken);

        return existingCampaignCount == campaignIds.Count
            ? null
            : ApplicationStatusCode.CampaignNotFound;
    }

    private static bool TryBuildAsset(
        int? parentAssetId,
        string? nameValue,
        string? descriptionValue,
        AssetType assetType,
        ItemType? itemType,
        List<Guid>? campaignIds,
        DateTimeOffset timestamp,
        out Asset asset)
    {
        asset = new Asset();

        var name = nameValue?.Trim();
        var description = string.IsNullOrWhiteSpace(descriptionValue)
            ? string.Empty
            : descriptionValue.Trim();

        if (parentAssetId < 1
            || string.IsNullOrWhiteSpace(name)
            || name.Length > MaximumAssetNameLength
            || description.Length > MaximumAssetDescriptionLength)
        {
            return false;
        }

        asset = new Asset
        {
            ParentAssetId = parentAssetId,
            AssetType = assetType,
            Name = name,
            Description = description,
            ItemType = itemType,
            CampaignIds = NormalizeCampaignIds(campaignIds),
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        return true;
    }

    private static List<Guid>? NormalizeCampaignIds(List<Guid>? campaignIds)
    {
        return campaignIds is null
            ? null
            : campaignIds.Distinct().ToList();
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static AssetResponse ToResponse(Asset asset)
    {
        return new AssetResponse
        {
            Id = asset.Id,
            ParentAssetId = asset.ParentAssetId,
            AssetType = asset.AssetType,
            Name = asset.Name,
            Description = asset.Description,
            ItemType = asset.ItemType,
            CampaignIds = asset.CampaignIds,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }
}
