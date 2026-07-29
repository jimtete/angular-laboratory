using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.StoryBeatIndexPathRuleRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.StoryBlockRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.StoryBeatIndexPathRuleService;

public sealed class StoryBeatIndexPathRuleService : IStoryBeatIndexPathRuleService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IStoryBeatIndexPathRuleRepository _storyBeatIndexPathRuleRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IStoryBlockRepository _storyBlockRepository;
    private readonly IUserRepository _userRepository;

    public StoryBeatIndexPathRuleService(
        ICampaignRepository campaignRepository,
        IStoryBeatIndexPathRuleRepository storyBeatIndexPathRuleRepository,
        IStoryBeatRepository storyBeatRepository,
        IStoryBlockRepository storyBlockRepository,
        IUserRepository userRepository)
    {
        _campaignRepository = campaignRepository;
        _storyBeatIndexPathRuleRepository = storyBeatIndexPathRuleRepository;
        _storyBeatRepository = storyBeatRepository;
        _storyBlockRepository = storyBlockRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBeatIndexPathRuleResponse>>> ListByStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignStoryBlockAccessAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatIndexPathRuleResponse>>(
                validationStatusCode.Value);
        }

        var rules = await _storyBeatIndexPathRuleRepository.ListByStoryBlockAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBeatIndexPathRuleResponse>>(
            ApplicationStatusCode.Success,
            rules.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<StoryBeatIndexPathRuleResponse>> GetByOrderIndexAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        if (orderIndex <= 0)
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateCampaignStoryBlockAccessAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                validationStatusCode.Value);
        }

        var rule = await _storyBeatIndexPathRuleRepository.GetByCampaignStoryBlockAndOrderIndexAsync(
            campaignId,
            storyBlockId,
            orderIndex,
            cancellationToken);

        return rule is null
            ? new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.StoryBeatIndexPathRuleNotFound)
            : new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.Success,
                ToResponse(rule));
    }

    public async Task<ServiceResult<StoryBeatIndexPathRuleResponse>> UpsertAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        UpsertStoryBeatIndexPathRuleRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (orderIndex <= 0)
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.InvalidStoryBeat);
        }

        if (request is null || !Enum.IsDefined(request.RelationType))
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.StoryBeatIndexPathRuleInvalidRelationType);
        }

        var validationStatusCode = await ValidateCampaignStoryBlockAccessAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                validationStatusCode.Value);
        }

        var storyBeatCount = await _storyBeatRepository.CountByStoryBlockIdAndOrderIndexAsync(
            storyBlockId,
            orderIndex,
            cancellationToken);

        if (storyBeatCount < 2)
        {
            return new ServiceResult<StoryBeatIndexPathRuleResponse>(
                ApplicationStatusCode.StoryBeatIndexPathRuleRequiresMultipleStoryBeats);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var rule = await _storyBeatIndexPathRuleRepository.GetByCampaignStoryBlockAndOrderIndexAsync(
            campaignId,
            storyBlockId,
            orderIndex,
            cancellationToken);

        if (rule is null)
        {
            rule = new StoryBeatIndexPathRule
            {
                Id = Guid.NewGuid(),
                CampaignId = campaignId,
                StoryBlockId = storyBlockId,
                OrderIndex = orderIndex,
                CreatedAtUtc = timestamp
            };

            await _storyBeatIndexPathRuleRepository.AddAsync(rule, cancellationToken);
        }

        rule.RelationType = request.RelationType;
        rule.IsRequired = request.IsRequired;
        rule.UpdatedAtUtc = timestamp;

        await _storyBeatIndexPathRuleRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatIndexPathRuleResponse>(
            ApplicationStatusCode.Success,
            ToResponse(rule));
    }

    public async Task<ServiceResult<object>> DeleteAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        if (orderIndex <= 0)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateCampaignStoryBlockAccessAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(
                validationStatusCode.Value);
        }

        var rule = await _storyBeatIndexPathRuleRepository.GetByCampaignStoryBlockAndOrderIndexAsync(
            campaignId,
            storyBlockId,
            orderIndex,
            cancellationToken);

        if (rule is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.StoryBeatIndexPathRuleNotFound);
        }

        _storyBeatIndexPathRuleRepository.Remove(rule);
        await _storyBeatIndexPathRuleRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(
            ApplicationStatusCode.Success,
            new object());
    }

    private async Task<ApplicationStatusCode?> ValidateCampaignStoryBlockAccessAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
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

        if (campaign is null)
        {
            return ApplicationStatusCode.CampaignNotFound;
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is not null)
        {
            return null;
        }

        return await _storyBlockRepository.ExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken)
            ? ApplicationStatusCode.StoryBeatIndexPathRuleStoryBlockMismatch
            : ApplicationStatusCode.StoryBlockNotFound;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static StoryBeatIndexPathRuleResponse ToResponse(StoryBeatIndexPathRule rule)
    {
        return new StoryBeatIndexPathRuleResponse
        {
            Id = rule.Id,
            CampaignId = rule.CampaignId,
            StoryBlockId = rule.StoryBlockId,
            OrderIndex = rule.OrderIndex,
            RelationType = rule.RelationType,
            IsRequired = rule.IsRequired,
            CreatedAtUtc = rule.CreatedAtUtc,
            UpdatedAtUtc = rule.UpdatedAtUtc
        };
    }
}
