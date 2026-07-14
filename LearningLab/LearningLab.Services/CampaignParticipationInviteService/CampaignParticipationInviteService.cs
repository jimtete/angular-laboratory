using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Models.Notifications;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.NotificationCommandRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignParticipationInviteService;

public sealed class CampaignParticipationInviteService : ICampaignParticipationInviteService
{
    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly INotificationCommandRepository _notificationCommandRepository;
    private readonly IUserRepository _userRepository;

    public CampaignParticipationInviteService(
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        ICampaignRepository campaignRepository,
        INotificationCommandRepository notificationCommandRepository,
        IUserRepository userRepository)
    {
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _campaignRepository = campaignRepository;
        _notificationCommandRepository = notificationCommandRepository;
        _userRepository = userRepository;
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

        var invite = new CampaignParticipationInvite
        {
            CampaignId = campaignId,
            UserId = player.UserId,
            DateInvited = DateTimeOffset.UtcNow
        };

        await _campaignParticipationInviteRepository.ExecuteInTransactionAsync(
            async () =>
            {
                await _campaignParticipationInviteRepository.AddAsync(invite, cancellationToken);
                await _campaignParticipationInviteRepository.SaveChangesAsync(cancellationToken);

                await _notificationCommandRepository.CreateNotificationAsync(
                    Guid.NewGuid(),
                    player.UserId,
                    NotificationType.CampaignInvite,
                    BuildCampaignInviteNotificationDescription(user.Username, campaign.CampaignName),
                    DateTimeOffset.UtcNow,
                    cancellationToken);
            },
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
}
