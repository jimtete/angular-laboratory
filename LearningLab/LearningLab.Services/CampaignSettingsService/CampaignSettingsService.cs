using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSettingsRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignSettingsService;

public sealed class CampaignSettingsService : ICampaignSettingsService
{
    private const int DefaultMaxNumberOfPlayers = 1;

    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignSettingsRepository _campaignSettingsRepository;
    private readonly IUserRepository _userRepository;

    public CampaignSettingsService(
        ICampaignRepository campaignRepository,
        ICampaignSettingsRepository campaignSettingsRepository,
        IUserRepository userRepository)
    {
        _campaignRepository = campaignRepository;
        _campaignSettingsRepository = campaignSettingsRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<CampaignSettingsResponse>> GetCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var settings = await GetOrCreateCampaignSettingsAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<CampaignSettingsResponse>(
            ApplicationStatusCode.Success,
            ToResponse(settings));
    }

    public async Task<ServiceResult<CampaignSettingsResponse>> UpdateCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        UpdateCampaignSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.MaxNumberOfPlayers < DefaultMaxNumberOfPlayers)
        {
            return new ServiceResult<CampaignSettingsResponse>(
                ApplicationStatusCode.InvalidCampaignSettings);
        }

        var validationResult = await ValidateCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var settings = await GetOrCreateCampaignSettingsAsync(
            campaignId,
            cancellationToken);

        settings.MaxNumberOfPlayers = request.MaxNumberOfPlayers;
        _campaignSettingsRepository.Update(settings);
        await _campaignSettingsRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignSettingsResponse>(
            ApplicationStatusCode.Success,
            ToResponse(settings));
    }

    private async Task<ServiceResult<CampaignSettingsResponse>?> ValidateCampaignAccessAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<CampaignSettingsResponse>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return new ServiceResult<CampaignSettingsResponse>(
                ApplicationStatusCode.CampaignMasterRoleRequired);
        }

        var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
            campaignId,
            userId,
            cancellationToken);

        return campaign is null
            ? new ServiceResult<CampaignSettingsResponse>(ApplicationStatusCode.CampaignNotFound)
            : null;
    }

    private async Task<CampaignSettings> GetOrCreateCampaignSettingsAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var settings = await _campaignSettingsRepository.GetByCampaignIdAsync(
            campaignId,
            cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new CampaignSettings
        {
            CampaignId = campaignId,
            MaxNumberOfPlayers = DefaultMaxNumberOfPlayers
        };

        await _campaignSettingsRepository.AddAsync(settings, cancellationToken);
        await _campaignSettingsRepository.SaveChangesAsync(cancellationToken);

        return settings;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static CampaignSettingsResponse ToResponse(CampaignSettings settings)
    {
        return new CampaignSettingsResponse
        {
            CampaignId = settings.CampaignId,
            MaxNumberOfPlayers = settings.MaxNumberOfPlayers
        };
    }
}
