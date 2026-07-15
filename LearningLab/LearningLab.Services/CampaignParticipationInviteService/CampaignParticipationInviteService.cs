using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Models.Notifications;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSettingsRepository;
using LearningLab.Data.Repositories.NotificationCommandRepository;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.Notifications;
using LearningLab.Data.Models.DTOs.Notifications;

namespace LearningLab.Services.CampaignParticipationInviteService;

public sealed class CampaignParticipationInviteService : ICampaignParticipationInviteService
{
    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly IApplicationEventHub _applicationEventHub;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignSettingsRepository _campaignSettingsRepository;
    private readonly INotificationCommandRepository _notificationCommandRepository;
    private readonly IUserRepository _userRepository;

    public CampaignParticipationInviteService(
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        IApplicationEventHub applicationEventHub,
        ICampaignRepository campaignRepository,
        ICampaignSettingsRepository campaignSettingsRepository,
        INotificationCommandRepository notificationCommandRepository,
        IUserRepository userRepository)
    {
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _applicationEventHub = applicationEventHub;
        _campaignRepository = campaignRepository;
        _campaignSettingsRepository = campaignSettingsRepository;
        _notificationCommandRepository = notificationCommandRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignPendingInviteResponse>>> GetPendingInvitesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<IReadOnlyList<CampaignPendingInviteResponse>>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Player))
        {
            return new ServiceResult<IReadOnlyList<CampaignPendingInviteResponse>>(
                ApplicationStatusCode.CampaignInvitePlayerRoleRequired);
        }

        var invites = await _campaignParticipationInviteRepository.ListPendingByUserIdAsync(
            userId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignPendingInviteResponse>>(
            ApplicationStatusCode.Success,
            invites.Select(ToPendingInviteResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<string>>> GetCampaignMemberUsernamesAsync(
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
            return new ServiceResult<IReadOnlyList<string>>(validationStatusCode.Value);
        }

        var usernames = await _campaignParticipationInviteRepository.ListParticipantUsernamesByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<string>>(
            ApplicationStatusCode.Success,
            usernames);
    }

    public async Task<ServiceResult<IReadOnlyList<string>>> GetCampaignInviteUsernamesAsync(
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
            return new ServiceResult<IReadOnlyList<string>>(validationStatusCode.Value);
        }

        var usernames = await _campaignParticipationInviteRepository.ListInviteUsernamesByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<string>>(
            ApplicationStatusCode.Success,
            usernames);
    }

    public async Task<ServiceResult<CampaignParticipationInviteResponse>> InvitePlayerAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignParticipationInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.InvalidCampaignInvite);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignMasterRoleRequired);
        }

        var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
            campaignId,
            userId,
            cancellationToken);

        if (campaign is null)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignNotFound);
        }

        var player = await _userRepository.GetByUsernameAsync(
            request.Username.Trim(),
            cancellationToken);

        if (player is null)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(player, AccessRoleNames.Player))
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignInvitePlayerRoleRequired);
        }

        var playerAlreadyJoined = await _campaignParticipationInviteRepository.ExistsParticipationAsync(
            campaignId,
            player.UserId,
            cancellationToken);

        if (playerAlreadyJoined)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignParticipantAlreadyExists);
        }

        var inviteAlreadyExists = await _campaignParticipationInviteRepository.ExistsInviteAsync(
            campaignId,
            player.UserId,
            cancellationToken);

        if (inviteAlreadyExists)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignInviteAlreadyExists);
        }

        var campaignSettings = await _campaignSettingsRepository.GetByCampaignIdAsync(
            campaignId,
            cancellationToken);
        var maxNumberOfPlayers = campaignSettings?.MaxNumberOfPlayers ?? 1;
        var reservedPlayerSlots = await _campaignParticipationInviteRepository.CountReservedPlayerSlotsByCampaignIdAsync(
            campaignId,
            cancellationToken);

        if (reservedPlayerSlots >= maxNumberOfPlayers)
        {
            return new ServiceResult<CampaignParticipationInviteResponse>(
                ApplicationStatusCode.CampaignPlayerLimitReached);
        }

        var invite = new CampaignParticipationInvite
        {
            CampaignId = campaignId,
            UserId = player.UserId,
            DateInvited = DateTimeOffset.UtcNow
        };
        var notification = new NotificationResponse
        {
            NotificationId = Guid.NewGuid(),
            UserId = player.UserId,
            NotificationType = NotificationType.CampaignInvite,
            Description = BuildCampaignInviteNotificationDescription(user.Username, campaign.CampaignName),
            DateCreated = DateTimeOffset.UtcNow,
            DateRead = null
        };

        await _campaignParticipationInviteRepository.ExecuteInTransactionAsync(
            async () =>
            {
                await _campaignParticipationInviteRepository.AddAsync(invite, cancellationToken);
                await _campaignParticipationInviteRepository.SaveChangesAsync(cancellationToken);

                await _notificationCommandRepository.CreateNotificationAsync(
                    notification.NotificationId,
                    notification.UserId,
                    notification.NotificationType,
                    notification.Description,
                    notification.DateCreated,
                    cancellationToken);
            },
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new NotificationCreatedEvent(notification),
            cancellationToken);

        return new ServiceResult<CampaignParticipationInviteResponse>(
            ApplicationStatusCode.Success,
            new CampaignParticipationInviteResponse
            {
                CampaignId = invite.CampaignId,
                UserId = invite.UserId,
                Username = player.Username,
                DateInvited = invite.DateInvited
            });
    }

    public Task<ServiceResult<CampaignInviteResolutionResponse>> AcceptInviteAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return ResolveInviteAsync(
            userId,
            campaignId,
            acceptInvite: true,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignInviteResolutionResponse>> RejectInviteAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return ResolveInviteAsync(
            userId,
            campaignId,
            acceptInvite: false,
            cancellationToken);
    }

    private async Task<ServiceResult<CampaignInviteResolutionResponse>> ResolveInviteAsync(
        Guid userId,
        Guid campaignId,
        bool acceptInvite,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<CampaignInviteResolutionResponse>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Player))
        {
            return new ServiceResult<CampaignInviteResolutionResponse>(
                ApplicationStatusCode.CampaignInvitePlayerRoleRequired);
        }

        var campaign = await _campaignRepository.GetByIdAsync(campaignId, cancellationToken);

        if (campaign is null)
        {
            return new ServiceResult<CampaignInviteResolutionResponse>(
                ApplicationStatusCode.CampaignNotFound);
        }

        var invite = await _campaignParticipationInviteRepository.GetInviteAsync(
            campaignId,
            userId,
            cancellationToken);

        if (invite is null)
        {
            return new ServiceResult<CampaignInviteResolutionResponse>(
                ApplicationStatusCode.CampaignInviteNotFound);
        }

        if (acceptInvite)
        {
            var playerAlreadyJoined = await _campaignParticipationInviteRepository.ExistsParticipationAsync(
                campaignId,
                userId,
                cancellationToken);

            if (playerAlreadyJoined)
            {
                return new ServiceResult<CampaignInviteResolutionResponse>(
                    ApplicationStatusCode.CampaignParticipantAlreadyExists);
            }
        }

        var dateResolved = DateTimeOffset.UtcNow;
        IReadOnlyList<Guid> deletedNotificationIds = [];
        NotificationResponse? acceptedInviteNotification = acceptInvite
            ? new NotificationResponse
            {
                NotificationId = Guid.NewGuid(),
                UserId = campaign.GameMasterId,
                NotificationType = NotificationType.Information,
                Description = BuildCampaignInviteAcceptedNotificationDescription(
                    user.Username,
                    campaign.CampaignName),
                DateCreated = dateResolved,
                DateRead = null
            }
            : null;

        await _campaignParticipationInviteRepository.ExecuteInTransactionAsync(
            async () =>
            {
                if (acceptInvite)
                {
                    await _campaignParticipationInviteRepository.AddParticipationAsync(
                        new PlayerCampaignParticipation
                        {
                            CampaignId = campaignId,
                            UserId = userId,
                            DateJoined = dateResolved
                        },
                        cancellationToken);
                }

                _campaignParticipationInviteRepository.RemoveInvite(invite);
                await _campaignParticipationInviteRepository.SaveChangesAsync(cancellationToken);

                deletedNotificationIds = await _notificationCommandRepository.SoftDeleteNotificationsAsync(
                    userId,
                    NotificationType.CampaignInvite,
                    BuildCampaignInviteNotificationDescription(
                        campaign.GameMaster.Username,
                        campaign.CampaignName),
                    dateResolved,
                    cancellationToken);

                if (acceptedInviteNotification is not null)
                {
                    await _notificationCommandRepository.CreateNotificationAsync(
                        acceptedInviteNotification.NotificationId,
                        acceptedInviteNotification.UserId,
                        acceptedInviteNotification.NotificationType,
                        acceptedInviteNotification.Description,
                        acceptedInviteNotification.DateCreated,
                        cancellationToken);
                }
            },
            cancellationToken);

        if (acceptedInviteNotification is not null)
        {
            await _applicationEventHub.PublishAsync(
                new NotificationCreatedEvent(acceptedInviteNotification),
                cancellationToken);
        }

        if (deletedNotificationIds.Count > 0)
        {
            await _applicationEventHub.PublishAsync(
                new NotificationDeletedEvent(userId, deletedNotificationIds),
                cancellationToken);
        }

        return new ServiceResult<CampaignInviteResolutionResponse>(
            ApplicationStatusCode.Success,
            new CampaignInviteResolutionResponse
            {
                CampaignId = campaignId,
                UserId = userId,
                Accepted = acceptInvite,
                DateResolved = dateResolved
            });
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

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildCampaignInviteNotificationDescription(
        string gameMasterUsername,
        string campaignName)
    {
        return $"{gameMasterUsername} invited you to join campaign \"{campaignName}\".";
    }

    private static string BuildCampaignInviteAcceptedNotificationDescription(
        string playerUsername,
        string campaignName)
    {
        return $"{playerUsername} has accepted the invite to join campaign \"{campaignName}\".";
    }

    private static CampaignPendingInviteResponse ToPendingInviteResponse(
        CampaignParticipationInvite invite)
    {
        return new CampaignPendingInviteResponse
        {
            CampaignId = invite.CampaignId,
            CampaignName = invite.Campaign.CampaignName,
            Version = invite.Campaign.Version,
            CampaignPictureUrl = invite.Campaign.CampaignPictureUrl,
            GameMasterId = invite.Campaign.GameMasterId,
            GameMasterUsername = invite.Campaign.GameMaster.Username,
            DateInvited = invite.DateInvited
        };
    }
}
