using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Stores;
using LearningLab.Data.Models.DTOs.Campaign.Stores;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSettingsRepository;
using LearningLab.Data.Repositories.CampaignStoreRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignStoreService;

public sealed class CampaignStoreService : ICampaignStoreService
{
    private const int MaximumStoreLocationLength = 256;
    private const int MaximumStoreNameLength = 256;
    private const int MaximumStoreDescriptionLength = 4096;
    private const int MaximumStoreItemNameLength = 256;
    private const int MaximumStoreItemDescriptionLength = 4096;

    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignSettingsRepository _campaignSettingsRepository;
    private readonly ICampaignStoreRepository _campaignStoreRepository;
    private readonly IUserRepository _userRepository;

    public CampaignStoreService(
        ICampaignRepository campaignRepository,
        ICampaignSettingsRepository campaignSettingsRepository,
        ICampaignStoreRepository campaignStoreRepository,
        IUserRepository userRepository)
    {
        _campaignRepository = campaignRepository;
        _campaignSettingsRepository = campaignSettingsRepository;
        _campaignStoreRepository = campaignStoreRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<StoreResponse>>> GetCampaignStoresAsync(
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
            return new ServiceResult<IReadOnlyList<StoreResponse>>(
                validationStatusCode.Value);
        }

        var stores = await _campaignStoreRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoreResponse>>(
            ApplicationStatusCode.Success,
            stores.Select(store => ToResponse(store)).ToList());
    }

    public async Task<ServiceResult<StoreResponse>> GetCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default)
    {
        if (storeId < 1)
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                validationStatusCode.Value);
        }

        var store = await _campaignStoreRepository.GetByCampaignIdAndStoreIdAsync(
            campaignId,
            storeId,
            cancellationToken);

        if (store is null)
        {
            return new ServiceResult<StoreResponse>(ApplicationStatusCode.StoreNotFound);
        }

        var storeMechanicsResult = await GetStoreMechanicsAsync(
            campaignId,
            cancellationToken);

        if (storeMechanicsResult.StatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                storeMechanicsResult.StatusCode.Value);
        }

        var response = await BuildStoreResponseAsync(
            campaignId,
            store,
            storeMechanicsResult.StoreMechanics,
            cancellationToken);

        return new ServiceResult<StoreResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public async Task<ServiceResult<StoreResponse>> CreateCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        CreateStoreRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildStore(
            campaignId,
            request,
            out var store))
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                validationStatusCode.Value);
        }

        await _campaignStoreRepository.AddAsync(store, cancellationToken);
        await _campaignStoreRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoreResponse>(
            ApplicationStatusCode.Success,
            ToResponse(store));
    }

    public async Task<ServiceResult<StoreResponse>> UpdateCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreRequest request,
        CancellationToken cancellationToken = default)
    {
        if (storeId < 1
            || !TryBuildStore(
                campaignId,
                request,
                out var updatedStore))
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                validationStatusCode.Value);
        }

        var store = await _campaignStoreRepository.GetMutableByCampaignIdAndStoreIdAsync(
            campaignId,
            storeId,
            cancellationToken);

        if (store is null)
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.StoreNotFound);
        }

        var existingItemsById = store.Items.ToDictionary(item => item.StoreItemId);
        if (updatedStore.Items.Any(item => item.StoreItemId > 0 && !existingItemsById.ContainsKey(item.StoreItemId)))
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        store.StoreType = updatedStore.StoreType;
        store.StoreLocation = updatedStore.StoreLocation;
        store.StoreName = updatedStore.StoreName;
        store.StoreDescription = updatedStore.StoreDescription;
        store.StoreDiscountPercentage = updatedStore.StoreDiscountPercentage;

        store.Items.Clear();
        foreach (var item in updatedStore.Items)
        {
            if (item.StoreItemId > 0
                && existingItemsById.TryGetValue(item.StoreItemId, out var existingItem))
            {
                item.TimesSold = existingItem.TimesSold;
            }

            store.Items.Add(item);
        }

        await _campaignStoreRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoreResponse>(
            ApplicationStatusCode.Success,
            ToResponse(store));
    }

    public async Task<ServiceResult<StoreResponse>> UpdateCampaignStoreItemPurchasesAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreItemPurchaseStateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (storeId < 1
            || request.Items is null
            || request.Items.Any(item => item.StoreItemId < 1 || item.TimesSold < 0)
            || request.Items.Select(item => item.StoreItemId).Distinct().Count() != request.Items.Count)
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                validationStatusCode.Value);
        }

        var store = await _campaignStoreRepository.GetMutableByCampaignIdAndStoreIdAsync(
            campaignId,
            storeId,
            cancellationToken);

        if (store is null)
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.StoreNotFound);
        }

        var itemsById = store.Items.ToDictionary(item => item.StoreItemId);

        foreach (var itemRequest in request.Items)
        {
            if (!itemsById.TryGetValue(itemRequest.StoreItemId, out var item))
            {
                return new ServiceResult<StoreResponse>(
                    ApplicationStatusCode.InvalidStore);
            }

            item.TimesSold = itemRequest.TimesSold;
        }

        await _campaignStoreRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoreResponse>(
            ApplicationStatusCode.Success,
            ToResponse(store));
    }

    public async Task<ServiceResult<StoreResponse>> UpdateCampaignStoreLockStateAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        UpdateStoreLockStateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (storeId < 1
            || request is null
            || !Enum.IsDefined(request.LockState))
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                validationStatusCode.Value);
        }

        var store = await _campaignStoreRepository.GetMutableByCampaignIdAndStoreIdAsync(
            campaignId,
            storeId,
            cancellationToken);

        if (store is null)
        {
            return new ServiceResult<StoreResponse>(
                ApplicationStatusCode.StoreNotFound);
        }

        store.LockState = request.LockState;

        await _campaignStoreRepository.SaveChangesAsync(cancellationToken);

        var storeMechanicsResult = await GetStoreMechanicsAsync(
            campaignId,
            cancellationToken);

        if (storeMechanicsResult.StatusCode is not null)
        {
            return new ServiceResult<StoreResponse>(
                storeMechanicsResult.StatusCode.Value);
        }

        var response = await BuildStoreResponseAsync(
            campaignId,
            store,
            storeMechanicsResult.StoreMechanics,
            cancellationToken);

        return new ServiceResult<StoreResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public async Task<ServiceResult<object>> DeleteCampaignStoreAsync(
        Guid userId,
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken = default)
    {
        if (storeId < 1)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.InvalidStore);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(
                validationStatusCode.Value);
        }

        var store = await _campaignStoreRepository.GetMutableByCampaignIdAndStoreIdAsync(
            campaignId,
            storeId,
            cancellationToken);

        if (store is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.StoreNotFound);
        }

        _campaignStoreRepository.Remove(store);
        await _campaignStoreRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
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

    private static bool TryBuildStore(
        Guid campaignId,
        CreateStoreRequest? request,
        out StoreEntry store)
    {
        store = new StoreEntry();

        if (request is null
            || !TryBuildStoreValues(
                request.StoreType,
                request.StoreLocation,
                request.StoreName,
                request.StoreDescription,
                request.StoreDiscountPercentage,
                request.Items,
                out var storeType,
                out var storeLocation,
                out var storeName,
                out var storeDescription,
                out var storeDiscountPercentage,
                out var items))
        {
            return false;
        }

        store = new StoreEntry
        {
            CampaignId = campaignId,
            StoreType = storeType,
            StoreLocation = storeLocation,
            StoreName = storeName,
            StoreDescription = storeDescription,
            StoreDiscountPercentage = storeDiscountPercentage,
            Items = items
        };

        return true;
    }

    private static bool TryBuildStore(
        Guid campaignId,
        UpdateStoreRequest? request,
        out StoreEntry store)
    {
        store = new StoreEntry();

        if (request is null
            || !TryBuildStoreValues(
                request.StoreType,
                request.StoreLocation,
                request.StoreName,
                request.StoreDescription,
                request.StoreDiscountPercentage,
                request.Items,
                out var storeType,
                out var storeLocation,
                out var storeName,
                out var storeDescription,
                out var storeDiscountPercentage,
                out var items))
        {
            return false;
        }

        store = new StoreEntry
        {
            CampaignId = campaignId,
            StoreType = storeType,
            StoreLocation = storeLocation,
            StoreName = storeName,
            StoreDescription = storeDescription,
            StoreDiscountPercentage = storeDiscountPercentage,
            Items = items
        };

        return true;
    }

    private static bool TryBuildStoreValues<TItemRequest>(
        StoreType storeType,
        string? storeLocationValue,
        string? storeNameValue,
        string? storeDescriptionValue,
        int storeDiscountPercentageValue,
        IReadOnlyList<TItemRequest>? itemRequests,
        out StoreType normalizedStoreType,
        out string storeLocation,
        out string? storeName,
        out string? storeDescription,
        out int storeDiscountPercentage,
        out List<StoreItem> items)
    {
        normalizedStoreType = storeType;
        storeLocation = storeLocationValue?.Trim() ?? string.Empty;
        storeName = NormalizeOptionalString(storeNameValue);
        storeDescription = NormalizeOptionalString(storeDescriptionValue);
        storeDiscountPercentage = storeDiscountPercentageValue;
        items = [];

        if (!Enum.IsDefined(storeType)
            || string.IsNullOrWhiteSpace(storeLocation)
            || storeLocation.Length > MaximumStoreLocationLength
            || storeName?.Length > MaximumStoreNameLength
            || storeDescription?.Length > MaximumStoreDescriptionLength
            || storeDiscountPercentage is < 0 or > 100
            || itemRequests is null
            || itemRequests.Count == 0
            || itemRequests.Any(item => item is null))
        {
            return false;
        }

        foreach (var itemRequest in itemRequests)
        {
            if (!TryBuildStoreItem(
                itemRequest,
                out var item))
            {
                return false;
            }

            items.Add(item);
        }

        return true;
    }

    private static bool TryBuildStoreItem<TItemRequest>(
        TItemRequest itemRequest,
        out StoreItem item)
    {
        item = new StoreItem();

        var values = itemRequest switch
        {
            CreateStoreItemRequest createRequest => (
                (long?)null,
                createRequest.Quantity,
                createRequest.ItemName,
                createRequest.ItemDescription,
                createRequest.ItemPrice,
                createRequest.ItemPriceDiscount,
                createRequest.ItemPricePercentageDiscount),
            UpdateStoreItemRequest updateRequest => (
                updateRequest.StoreItemId,
                updateRequest.Quantity,
                updateRequest.ItemName,
                updateRequest.ItemDescription,
                updateRequest.ItemPrice,
                updateRequest.ItemPriceDiscount,
                updateRequest.ItemPricePercentageDiscount),
            _ => (null, (int?)null, null, null, 0, 0, 0)
        };

        var itemName = values.Item3?.Trim();
        var itemDescription = NormalizeOptionalString(values.Item4);

        if (string.IsNullOrWhiteSpace(itemName)
            || itemName.Length > MaximumStoreItemNameLength
            || itemDescription?.Length > MaximumStoreItemDescriptionLength
            || values.Item2 < 0
            || values.Item5 < 0
            || values.Item6 < 0
            || values.Item6 > values.Item5
            || values.Item7 < 0
            || values.Item7 > 100)
        {
            return false;
        }

        item = new StoreItem
        {
            StoreItemId = values.Item1 ?? 0,
            Quantity = values.Item2,
            TimesSold = 0,
            ItemName = itemName,
            ItemDescription = itemDescription,
            ItemPrice = values.Item5,
            ItemPriceDiscount = values.Item6,
            ItemPricePercentageDiscount = values.Item7
        };

        return true;
    }

    private static string? NormalizeOptionalString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private async Task<(ApplicationStatusCode? StatusCode, StoreMechanics StoreMechanics)> GetStoreMechanicsAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var settings = await _campaignSettingsRepository.GetByCampaignIdAsync(
            campaignId,
            cancellationToken);
        var storeMechanics = settings?.StoreMechanics ?? StoreMechanics.GlobalStores;

        return Enum.IsDefined(storeMechanics)
            ? (null, storeMechanics)
            : (ApplicationStatusCode.InvalidCampaignSettings, storeMechanics);
    }

    private async Task<StoreResponse> BuildStoreResponseAsync(
        Guid campaignId,
        StoreEntry store,
        StoreMechanics storeMechanics,
        CancellationToken cancellationToken)
    {
        if (storeMechanics != StoreMechanics.UnlockingStores)
        {
            return ToResponse(store);
        }

        var unlockedStores = await _campaignStoreRepository.ListUnlockedByCampaignIdAndStoreTypeAsync(
            campaignId,
            store.StoreType,
            store.StoreId,
            cancellationToken);

        return ToResponse(
            store,
            unlockedStores.Select(unlockedStore => ToResponse(unlockedStore)).ToList());
    }

    private static StoreResponse ToResponse(
        StoreEntry store,
        IReadOnlyList<StoreResponse>? unlockedStores = null)
    {
        return new StoreResponse
        {
            StoreId = store.StoreId,
            CampaignId = store.CampaignId,
            StoreType = store.StoreType,
            LockState = store.LockState,
            StoreLocation = store.StoreLocation,
            StoreName = store.StoreName,
            StoreDescription = store.StoreDescription,
            StoreDiscountPercentage = store.StoreDiscountPercentage,
            Items = store.Items
                .OrderBy(item => item.ItemName)
                .ThenBy(item => item.StoreItemId)
                .Select(ToResponse)
                .ToList(),
            UnlockedStores = unlockedStores ?? []
        };
    }

    private static StoreItemResponse ToResponse(StoreItem item)
    {
        return new StoreItemResponse
        {
            StoreItemId = item.StoreItemId,
            StoreId = item.StoreId,
            Quantity = item.Quantity,
            TimesSold = item.TimesSold,
            ItemName = item.ItemName,
            ItemDescription = item.ItemDescription,
            ItemPrice = item.ItemPrice,
            ItemPriceDiscount = item.ItemPriceDiscount,
            ItemPricePercentageDiscount = item.ItemPricePercentageDiscount
        };
    }
}
