using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSettingsRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignSettingsService;

public sealed class CampaignSettingsService : ICampaignSettingsService
{
    private const int DefaultMaxNumberOfPlayers = 1;
    private const int MaxMemberNicknameLength = 128;

    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignSettingsRepository _campaignSettingsRepository;
    private readonly IUserRepository _userRepository;

    public CampaignSettingsService(
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        ICampaignRepository campaignRepository,
        ICampaignSettingsRepository campaignSettingsRepository,
        IUserRepository userRepository)
    {
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _campaignRepository = campaignRepository;
        _campaignSettingsRepository = campaignSettingsRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<CampaignInformationResponse>> GetCampaignInformationAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateCampaignAccessAsync<CampaignInformationResponse>(
            userId,
            campaignId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var joinedMembers = await _campaignParticipationInviteRepository.ListParticipantInformationByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<CampaignInformationResponse>(
            ApplicationStatusCode.Success,
            new CampaignInformationResponse
            {
                CampaignId = campaignId,
                JoinedMembers = joinedMembers
            });
    }

    public async Task<ServiceResult<CampaignSettingsResponse>> GetCampaignSettingsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateCampaignAccessAsync<CampaignSettingsResponse>(
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
        
        var validationResult = await ValidateCampaignAccessAsync<CampaignSettingsResponse>(
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
        settings.CampaignDescription = request.CampaignDescription;
        _campaignSettingsRepository.Update(settings);
        await _campaignSettingsRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignSettingsResponse>(
            ApplicationStatusCode.Success,
            ToResponse(settings));
    }

    public async Task<ServiceResult<CampaignMemberInformationResponse>> UpdateCampaignMemberNicknameAsync(
        Guid userId,
        Guid campaignId,
        string username,
        UpdateCampaignMemberNicknameRequest request,
        CancellationToken cancellationToken = default)
    {
        var nickname = string.IsNullOrWhiteSpace(request.Nickname)
            ? null
            : request.Nickname.Trim();

        if (nickname?.Length > MaxMemberNicknameLength)
        {
            return new ServiceResult<CampaignMemberInformationResponse>(
                ApplicationStatusCode.InvalidCampaignMemberNickname);
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return new ServiceResult<CampaignMemberInformationResponse>(
                ApplicationStatusCode.CampaignParticipantNotFound);
        }

        var validationResult = await ValidateCampaignAccessAsync<CampaignMemberInformationResponse>(
            userId,
            campaignId,
            cancellationToken);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var participation = await _campaignParticipationInviteRepository.GetParticipationByUsernameAsync(
            campaignId,
            username.Trim(),
            cancellationToken);

        if (participation is null)
        {
            return new ServiceResult<CampaignMemberInformationResponse>(
                ApplicationStatusCode.CampaignParticipantNotFound);
        }

        participation.Nickname = nickname;
        await _campaignParticipationInviteRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignMemberInformationResponse>(
            ApplicationStatusCode.Success,
            new CampaignMemberInformationResponse
            {
                Username = participation.User.Username,
                FirstName = participation.User.FirstName,
                LastName = participation.User.LastName,
                Nickname = participation.Nickname
            });
    }

    private async Task<ServiceResult<T>?> ValidateCampaignAccessAsync<T>(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<T>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return new ServiceResult<T>(
                ApplicationStatusCode.CampaignMasterRoleRequired);
        }

        var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
            campaignId,
            userId,
            cancellationToken);

        return campaign is null
            ? new ServiceResult<T>(ApplicationStatusCode.CampaignNotFound)
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
