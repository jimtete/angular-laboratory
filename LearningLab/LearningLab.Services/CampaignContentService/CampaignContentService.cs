using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Repositories.CampaignMilestoneRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignContentService;

public sealed class CampaignContentService : ICampaignContentService
{
    private readonly ICampaignMilestoneRepository _campaignMilestoneRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public CampaignContentService(
        ICampaignMilestoneRepository campaignMilestoneRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _campaignMilestoneRepository = campaignMilestoneRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetCampaignMilestonesAsync(
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
            return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
                validationStatusCode.Value);
        }

        var milestones = await _campaignMilestoneRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
            ApplicationStatusCode.Success,
            milestones.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetUnachievedCampaignMilestonesAsync(
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
            return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
                validationStatusCode.Value);
        }

        var milestones = await _campaignMilestoneRepository.ListUnachievedByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
            ApplicationStatusCode.Success,
            milestones.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignMilestoneResponse>> CreateCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildCampaignMilestone(
            campaignId,
            request,
            DateTimeOffset.UtcNow,
            out var milestone))
        {
            return new ServiceResult<CampaignMilestoneResponse>(
                ApplicationStatusCode.InvalidCampaignMilestone);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignMilestoneResponse>(
                validationStatusCode.Value);
        }

        await _campaignMilestoneRepository.AddAsync(
            milestone,
            cancellationToken);
        await _campaignMilestoneRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignMilestoneResponse>(
            ApplicationStatusCode.Success,
            ToResponse(milestone));
    }

    public async Task<ServiceResult<CampaignMilestoneResponse>> UpdateCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int milestoneId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        if (milestoneId < 1
            || !TryBuildCampaignMilestone(
                campaignId,
                request,
                DateTimeOffset.UtcNow,
                out var updatedMilestone))
        {
            return new ServiceResult<CampaignMilestoneResponse>(
                ApplicationStatusCode.InvalidCampaignMilestone);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignMilestoneResponse>(
                validationStatusCode.Value);
        }

        var milestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            milestoneId,
            cancellationToken);

        if (milestone is null)
        {
            return new ServiceResult<CampaignMilestoneResponse>(
                ApplicationStatusCode.CampaignMilestoneNotFound);
        }

        milestone.Title = updatedMilestone.Title;
        milestone.Description = updatedMilestone.Description;
        milestone.AchievedAt = updatedMilestone.AchievedAt;
        milestone.Importance = updatedMilestone.Importance;
        milestone.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignMilestoneRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignMilestoneResponse>(
            ApplicationStatusCode.Success,
            ToResponse(milestone));
    }

    public async Task<ServiceResult<object>> DeleteCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int milestoneId,
        CancellationToken cancellationToken = default)
    {
        if (milestoneId < 1)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.InvalidCampaignMilestone);
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

        var milestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            milestoneId,
            cancellationToken);

        if (milestone is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.CampaignMilestoneNotFound);
        }

        _campaignMilestoneRepository.Remove(milestone);
        await _campaignMilestoneRepository.SaveChangesAsync(cancellationToken);

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

    private static bool TryBuildCampaignMilestone(
        Guid campaignId,
        CampaignMilestoneRequest? request,
        DateTimeOffset timestamp,
        out CampaignMilestone milestone)
    {
        milestone = new CampaignMilestone();

        var title = request?.Title?.Trim();
        var description = string.IsNullOrWhiteSpace(request?.Description)
            ? null
            : request.Description.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(title)
            || !Enum.IsDefined(request.Importance))
        {
            return false;
        }

        milestone = new CampaignMilestone
        {
            CampaignId = campaignId,
            Title = title,
            Description = description,
            AchievedAt = request.AchievedAt,
            Importance = request.Importance,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        return true;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static CampaignMilestoneResponse ToResponse(CampaignMilestone milestone)
    {
        return new CampaignMilestoneResponse
        {
            Id = milestone.Id,
            CampaignId = milestone.CampaignId,
            Title = milestone.Title,
            Description = milestone.Description,
            AchievedAt = milestone.AchievedAt,
            Importance = milestone.Importance,
            CreatedAt = milestone.CreatedAt,
            UpdatedAt = milestone.UpdatedAt
        };
    }

}
