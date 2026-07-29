using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Data.Repositories.CampaignMilestoneRepository;
using LearningLab.Data.Repositories.CampaignNpcRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.MonsterRepository;
using LearningLab.Data.Repositories.StoryBeatIndexPathRuleRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.StoryBlockMilestoneRepository;
using LearningLab.Data.Repositories.StoryBlockRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignStoryService;

public sealed class CampaignStoryService : ICampaignStoryService
{
    private readonly record struct StoryBeatOrder(int OrderIndex, int SecondaryOrderIndex);

    private const int MaximumStoryBlockTitleLength = 256;
    private const int MaximumStoryBeatTitleLength = 256;
    private const int MaximumCampaignNpcTagLength = 128;
    private const int MaximumCampaignNpcNameLength = 256;
    private const int MaximumCampaignNpcDisplayNameLength = 256;
    private const int MaximumCampaignNpcDescriptionLength = 2048;
    private const int MaximumNarrativeParagraphCount = 10;
    private const int MaximumDecisionOptionCount = 20;
    private const int MaximumDecisionDescriptionLength = 2048;
    private const int MaximumDecisionOptionTitleLength = 256;
    private const int MaximumDecisionOptionDescriptionLength = 2048;
    private const int MaximumCombatDescriptionLength = 2048;
    private const int MaximumCombatRewardsLength = 2048;
    private const int MaximumCombatEnemyNpcCount = 50;
    private const int MaximumTransitionDescriptionLength = 2048;

    private readonly IStoryBlockRepository _storyBlockRepository;
    private readonly IStoryBeatIndexPathRuleRepository _storyBeatIndexPathRuleRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IStoryBlockMilestoneRepository _storyBlockMilestoneRepository;
    private readonly ICampaignMilestoneRepository _campaignMilestoneRepository;
    private readonly ICampaignNpcRepository _campaignNpcRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IMonsterRepository _monsterRepository;
    private readonly IUserRepository _userRepository;

    public CampaignStoryService(
        IStoryBlockRepository storyBlockRepository,
        IStoryBeatIndexPathRuleRepository storyBeatIndexPathRuleRepository,
        IStoryBeatRepository storyBeatRepository,
        IStoryBlockMilestoneRepository storyBlockMilestoneRepository,
        ICampaignMilestoneRepository campaignMilestoneRepository,
        ICampaignNpcRepository campaignNpcRepository,
        ICampaignRepository campaignRepository,
        IMonsterRepository monsterRepository,
        IUserRepository userRepository)
    {
        _storyBlockRepository = storyBlockRepository;
        _storyBeatIndexPathRuleRepository = storyBeatIndexPathRuleRepository;
        _storyBeatRepository = storyBeatRepository;
        _storyBlockMilestoneRepository = storyBlockMilestoneRepository;
        _campaignMilestoneRepository = campaignMilestoneRepository;
        _campaignNpcRepository = campaignNpcRepository;
        _campaignRepository = campaignRepository;
        _monsterRepository = monsterRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<StoryBlockResponse>> CreateStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        CreateStoryBlockRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBlockTitle(title))
        {
            return new ServiceResult<StoryBlockResponse>(ApplicationStatusCode.InvalidStoryBlock);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBlockResponse>(validationStatusCode.Value);
        }

        var latestOrderIndex = await _storyBlockRepository.GetLatestOrderIndexByCampaignIdAsync(
            campaignId,
            cancellationToken);

        var storyBlock = new StoryBlock
        {
            StoryBlockId = Guid.NewGuid(),
            CampaignId = campaignId,
            Title = title!,
            OrderIndex = (latestOrderIndex ?? 0) + 1
        };

        await _storyBlockRepository.AddAsync(storyBlock, cancellationToken);
        await _storyBlockRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBlockResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBlock));
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBlockResponse>>> GetStoryBlocksAsync(
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
            return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(validationStatusCode.Value);
        }

        var storyBlocks = await _storyBlockRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(
            ApplicationStatusCode.Success,
            storyBlocks.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<StoryBlockResponse>> UpdateStoryBlockTitleAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        UpdateStoryBlockTitleRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBlockTitle(title))
        {
            return new ServiceResult<StoryBlockResponse>(ApplicationStatusCode.InvalidStoryBlock);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBlockResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBlockResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        storyBlock.Title = title!;

        await _storyBlockRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBlockResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBlock));
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBlockResponse>>> ReorderStoryBlocksAsync(
        Guid userId,
        Guid campaignId,
        ReorderStoryBlocksRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request?.StoryBlockIds is null
            || request.StoryBlockIds.Count == 0
            || request.StoryBlockIds.Distinct().Count() != request.StoryBlockIds.Count)
        {
            return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(ApplicationStatusCode.InvalidStoryBlock);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(validationStatusCode.Value);
        }

        var storyBlocks = await _storyBlockRepository.ListTrackedByCampaignIdAsync(
            campaignId,
            cancellationToken);

        if (storyBlocks.Count != request.StoryBlockIds.Count
            || storyBlocks.Select(block => block.StoryBlockId).Except(request.StoryBlockIds).Any())
        {
            return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(ApplicationStatusCode.InvalidStoryBlock);
        }

        await ApplyStoryBlockOrderAsync(
            storyBlocks,
            request.StoryBlockIds,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBlockResponse>>(
            ApplicationStatusCode.Success,
            storyBlocks
                .OrderBy(block => block.OrderIndex)
                .Select(ToResponse)
                .ToList());
    }

    public async Task<ServiceResult<object>> DeleteStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.StoryBlockNotFound);
        }

        _storyBlockRepository.Remove(storyBlock);
        await _storyBlockRepository.SaveChangesAsync(cancellationToken);
        await CompactStoryBlockOrderAsync(campaignId, cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<StoryBlockMilestoneResponse>> AddStoryBlockMilestoneAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBlockMilestoneResponse>(validationStatusCode.Value);
        }

        var storyBlockValidationStatusCode = await ValidateStoryBlockAndMilestoneAsync(
            campaignId,
            storyBlockId,
            campaignMilestoneId,
            cancellationToken);

        if (storyBlockValidationStatusCode is not null)
        {
            return new ServiceResult<StoryBlockMilestoneResponse>(
                storyBlockValidationStatusCode.Value);
        }

        var existingLink = await _storyBlockMilestoneRepository
            .GetByCampaignMilestoneIdAsync(
                campaignMilestoneId,
                cancellationToken);

        if (existingLink is not null)
        {
            return new ServiceResult<StoryBlockMilestoneResponse>(
                ApplicationStatusCode.StoryBlockMilestoneAlreadyExists);
        }

        var latestOrderIndex = await _storyBlockMilestoneRepository
            .GetLatestOrderIndexByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken);

        var link = new StoryBlockMilestone
        {
            StoryBlockId = storyBlockId,
            CampaignMilestoneId = campaignMilestoneId,
            OrderIndex = (latestOrderIndex ?? 0) + 1
        };

        await _storyBlockMilestoneRepository.AddAsync(link, cancellationToken);
        await _storyBlockMilestoneRepository.SaveChangesAsync(cancellationToken);

        var createdLink = await _storyBlockMilestoneRepository
            .GetByStoryBlockIdAndCampaignMilestoneIdAsync(
                storyBlockId,
                campaignMilestoneId,
                cancellationToken);

        return new ServiceResult<StoryBlockMilestoneResponse>(
            ApplicationStatusCode.Success,
            ToResponse(createdLink!));
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>>> GetStoryBlockMilestonesAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>>(
                validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var links = await _storyBlockMilestoneRepository.ListByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>>(
            ApplicationStatusCode.Success,
            links.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>> GetAvailableStoryBlockMilestonesAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
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

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var milestones = await _campaignMilestoneRepository.ListUnlinkedByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignMilestoneResponse>>(
            ApplicationStatusCode.Success,
            milestones.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<object>> RemoveStoryBlockMilestoneAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var storyBlockValidationStatusCode = await ValidateStoryBlockAndMilestoneAsync(
            campaignId,
            storyBlockId,
            campaignMilestoneId,
            cancellationToken);

        if (storyBlockValidationStatusCode is not null)
        {
            return new ServiceResult<object>(storyBlockValidationStatusCode.Value);
        }

        var link = await _storyBlockMilestoneRepository
            .GetByStoryBlockIdAndCampaignMilestoneIdAsync(
                storyBlockId,
                campaignMilestoneId,
                cancellationToken);

        if (link is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.StoryBlockMilestoneNotFound);
        }

        var removedOrderIndex = link.OrderIndex;

        _storyBlockMilestoneRepository.Remove(link);
        await _storyBlockMilestoneRepository.SaveChangesAsync(cancellationToken);
        await _storyBlockMilestoneRepository.DecrementOrderAfterAsync(
            storyBlockId,
            removedOrderIndex,
            cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateInformationStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateInformationStoryBeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        if (!TryBuildStoryBeatInformation(request, out var information))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Information,
            Information = information,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
            Combat = null,
            Transition = null,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateNarrativeStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateNarrativeStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        if (!TryBuildStoryBeatNarrative(request, out var narrative))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Narrative,
            Information = null,
            Narrative = narrative,
            Roleplaying = null,
            Decision = null,
            Combat = null,
            Transition = null,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateRoleplayingStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        var roleplaying = await BuildStoryBeatRoleplayingAsync(
            campaignId,
            request,
            null,
            cancellationToken);

        if (roleplaying is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Roleplaying,
            Information = null,
            Narrative = null,
            Roleplaying = roleplaying,
            Decision = null,
            Combat = null,
            Transition = null,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateDecisionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateDecisionStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatDecision(request, null, out var decision))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Decision,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = decision,
            Combat = null,
            Transition = null,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateCombatStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateCombatStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatCombat(request, out var combat, out var monsterIds))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        if (!await AllMonstersAreEnabledForCampaignAsync(
                campaignId,
                monsterIds,
                cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.CampaignMonsterNotFound);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Combat,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
            Combat = combat,
            Transition = null,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateTransitionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateTransitionStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatTransition(request, out var transition))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionAlreadyExists);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            null,
            null,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Transition,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
            Combat = null,
            Transition = transition,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        var storyBeats = await _storyBeatRepository.ListByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat, storyBeats));
    }

    public async Task<ServiceResult<StoryBeatResponse>> CreateMilestoneStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CreateMilestoneStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title) || request is null || request.MilestoneId < 1)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var milestoneId = request.MilestoneId;

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        if (await _storyBeatRepository.TransitionExistsByStoryBlockIdAsync(
                storyBlockId,
                cancellationToken: cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        var milestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            milestoneId,
            cancellationToken);

        if (milestone is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.CampaignMilestoneNotFound);
        }

        var existingMilestoneBeat = await _storyBeatRepository.GetByCampaignIdAndCampaignMilestoneIdAsync(
            campaignId,
            milestoneId,
            cancellationToken);

        if (existingMilestoneBeat is not null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatMilestoneAlreadyExists);
        }

        var order = await ResolveCreateStoryBeatOrderAsync(
            storyBlockId,
            request?.OrderIndex,
            request?.SecondaryOrderIndex,
            cancellationToken);

        if (order is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = order.Value.OrderIndex,
            SecondaryOrderIndex = order.Value.SecondaryOrderIndex,
            Title = title!,
            StoryBeatType = StoryBeatType.Milestone,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
            Combat = null,
            Transition = null,
            CampaignMilestoneId = milestoneId,
            Milestone = milestone
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    private async Task<StoryBeatOrder?> ResolveCreateStoryBeatOrderAsync(
        Guid storyBlockId,
        int? requestedOrderIndex,
        int? requestedSecondaryOrderIndex,
        CancellationToken cancellationToken)
    {
        if (requestedOrderIndex is <= 0 || requestedSecondaryOrderIndex is <= 0)
        {
            return null;
        }

        if (requestedOrderIndex is null && requestedSecondaryOrderIndex is not null)
        {
            return null;
        }

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        if (requestedOrderIndex is null)
        {
            return new StoryBeatOrder((latestOrderIndex ?? 0) + 1, 1);
        }

        if (requestedOrderIndex > (latestOrderIndex ?? 0) + 1)
        {
            return null;
        }

        var latestSecondaryOrderIndex = await _storyBeatRepository
            .GetLatestSecondaryOrderIndexByStoryBlockIdAndOrderIndexAsync(
                storyBlockId,
                requestedOrderIndex.Value,
                cancellationToken);

        if (latestSecondaryOrderIndex is null)
        {
            return requestedSecondaryOrderIndex is null or 1
                ? new StoryBeatOrder(requestedOrderIndex.Value, 1)
                : null;
        }

        if (requestedSecondaryOrderIndex is null)
        {
            return new StoryBeatOrder(requestedOrderIndex.Value, latestSecondaryOrderIndex.Value + 1);
        }

        if (await _storyBeatRepository.OrderExistsAsync(
                storyBlockId,
                requestedOrderIndex.Value,
                requestedSecondaryOrderIndex.Value,
                cancellationToken))
        {
            return null;
        }

        return new StoryBeatOrder(requestedOrderIndex.Value, requestedSecondaryOrderIndex.Value);
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateInformationStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateInformationStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Information
            || !TryBuildStoryBeatInformation(request, out var information))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        storyBeat.Title = title!;
        storyBeat.Information = information;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = null;
        storyBeat.Combat = null;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateNarrativeStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateNarrativeStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Narrative
            || !TryBuildStoryBeatNarrative(request, out var narrative))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = narrative;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = null;
        storyBeat.Combat = null;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateRoleplayingStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        var roleplaying = await BuildStoryBeatRoleplayingAsync(
            campaignId,
            request,
            storyBeat.Roleplaying,
            cancellationToken);

        if (storyBeat.StoryBeatType != StoryBeatType.Roleplaying
            || roleplaying is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = roleplaying;
        storyBeat.Decision = null;
        storyBeat.Combat = null;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateDecisionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateDecisionStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Decision)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        if (!TryBuildStoryBeatDecision(request, storyBeat.Decision, out var decision))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = decision;
        storyBeat.Combat = null;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateCombatStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateCombatStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatCombat(request, out var combat, out var monsterIds))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Combat)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        if (!await AllMonstersAreEnabledForCampaignAsync(
                campaignId,
                monsterIds,
                cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.CampaignMonsterNotFound);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = null;
        storyBeat.Combat = combat;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateTransitionStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateTransitionStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatTransition(request, out var transition))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Transition)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        if (await _storyBeatRepository.HasStoryBeatAfterAsync(
                storyBlockId,
                storyBeat.OrderIndex,
                storyBeat.SecondaryOrderIndex,
                cancellationToken))
        {
            return new ServiceResult<StoryBeatResponse>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = null;
        storyBeat.Combat = null;
        storyBeat.Transition = transition;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        var storyBeats = await _storyBeatRepository.ListByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat, storyBeats));
    }

    public async Task<ServiceResult<StoryBeatResponse>> UpdateMilestoneStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateMilestoneStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var title = request?.Title?.Trim();

        if (!IsValidStoryBeatTitle(title) || request is null || request.MilestoneId < 1)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatResponse>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Milestone)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var milestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            request!.MilestoneId,
            cancellationToken);

        if (milestone is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.CampaignMilestoneNotFound);
        }

        var existingMilestoneBeat = await _storyBeatRepository.GetByCampaignIdAndCampaignMilestoneIdAsync(
            campaignId,
            request.MilestoneId,
            cancellationToken);

        if (existingMilestoneBeat is not null && existingMilestoneBeat.Id != storyBeatId)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatMilestoneAlreadyExists);
        }

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = null;
        storyBeat.Combat = null;
        storyBeat.Transition = null;
        storyBeat.CampaignMilestoneId = request.MilestoneId;
        storyBeat.Milestone = milestone;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
    }

    public async Task<ServiceResult<object>> DeleteStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByStoryBlockIdAndStoryBeatIdAsync(
            storyBlockId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.StoryBeatNotFound);
        }

        var removedOrderIndex = storyBeat.OrderIndex;
        var removedSecondaryOrderIndex = storyBeat.SecondaryOrderIndex;

        _storyBeatRepository.Remove(storyBeat);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);
        await _storyBeatRepository.DecrementOrderAfterAsync(
            storyBlockId,
            removedOrderIndex,
            removedSecondaryOrderIndex,
            cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBeatResponse>>> ReorderStoryBeatsAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        ReorderStoryBeatsRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidReorderStoryBeatsRequest(request))
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
                ApplicationStatusCode.InvalidStoryBeat);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeats = await _storyBeatRepository.ListTrackedByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);
        var requestedStoryBeatIds = request!.StoryBeats
            .Select(beat => beat.StoryBeatId)
            .ToList();

        if (storyBeats.Count != request.StoryBeats.Count
            || storyBeats.Select(beat => beat.Id).Except(requestedStoryBeatIds).Any())
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
                ApplicationStatusCode.InvalidStoryBeat);
        }

        if (!TransitionWouldRemainFinal(storyBeats, request.StoryBeats))
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
                ApplicationStatusCode.StoryBeatTransitionMustBeFinal);
        }

        await ApplyStoryBeatOrderAsync(
            storyBeats,
            request.StoryBeats,
            cancellationToken);

        var orderedStoryBeats = storyBeats
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.Id)
            .ToList();
        var indexPathRules = await GetStoryBeatIndexPathRulesByOrderIndexAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
            ApplicationStatusCode.Success,
            orderedStoryBeats
                .Select(storyBeat => ToResponse(
                    storyBeat,
                    orderedStoryBeats,
                    indexPathRules))
                .ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBeatResponse>>> GetStoryBeatsAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(validationStatusCode.Value);
        }

        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeats = await _storyBeatRepository.ListByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);
        var indexPathRules = await GetStoryBeatIndexPathRulesByOrderIndexAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
            ApplicationStatusCode.Success,
            storyBeats
                .Select(storyBeat => ToResponse(
                    storyBeat,
                    storyBeats,
                    indexPathRules))
                .ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignNpcResponse>>>
        GetRoleplayingStoryBeatNpcsAsync(
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
            return new ServiceResult<IReadOnlyList<CampaignNpcResponse>>(
                validationStatusCode.Value);
        }

        var npcs = await _campaignNpcRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignNpcResponse>>(
            ApplicationStatusCode.Success,
            npcs.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignNpcResponse>> CreateCampaignNpcAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignNpcRequest? request,
        CancellationToken cancellationToken = default)
    {
        var tag = NormalizeCampaignNpcTag(request?.Tag);
        var name = request?.Name?.Trim();
        var displayName = ResolveCampaignNpcDisplayName(
            request?.DisplayName,
            request?.Nickname,
            name);
        var description = request?.Description?.Trim() ?? string.Empty;

        if (!IsValidCampaignNpc(tag, name, displayName, description))
        {
            return new ServiceResult<CampaignNpcResponse>(ApplicationStatusCode.InvalidCampaignNpc);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignNpcResponse>(validationStatusCode.Value);
        }

        var exists = await _campaignNpcRepository.ExistsByCampaignIdAndTagAsync(
            campaignId,
            tag!,
            cancellationToken);

        if (exists)
        {
            return new ServiceResult<CampaignNpcResponse>(ApplicationStatusCode.CampaignNpcAlreadyExists);
        }

        var npc = new CampaignNpc
        {
            CampaignNpcId = Guid.NewGuid(),
            CampaignId = campaignId,
            Tag = tag!,
            Name = name!,
            DisplayName = displayName!,
            Description = description
        };

        await _campaignNpcRepository.AddAsync(npc, cancellationToken);
        await _campaignNpcRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignNpcResponse>(
            ApplicationStatusCode.Success,
            ToResponse(npc));
    }

    public async Task<ServiceResult<CampaignNpcResponse>> UpdateCampaignNpcAsync(
        Guid userId,
        Guid campaignId,
        string npcTag,
        UpdateCampaignNpcRequest? request,
        CancellationToken cancellationToken = default)
    {
        var tag = NormalizeCampaignNpcTag(npcTag);
        var displayName = ResolveCampaignNpcDisplayName(
            request?.DisplayName,
            request?.Nickname,
            request?.Name);

        if (request is null
            || string.IsNullOrWhiteSpace(tag)
            || string.IsNullOrWhiteSpace(displayName)
            || displayName.Length > MaximumCampaignNpcDisplayNameLength
            || request.Description?.Trim().Length > MaximumCampaignNpcDescriptionLength)
        {
            return new ServiceResult<CampaignNpcResponse>(ApplicationStatusCode.InvalidCampaignNpc);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignNpcResponse>(validationStatusCode.Value);
        }

        var npc = await _campaignNpcRepository.GetByCampaignIdAndTagAsync(
            campaignId,
            tag!,
            cancellationToken);

        if (npc is null)
        {
            return new ServiceResult<CampaignNpcResponse>(ApplicationStatusCode.CampaignNpcNotFound);
        }

        var description = request.Description is null
            ? npc.Description
            : request.Description.Trim();

        npc.DisplayName = displayName!;
        npc.Description = description;
        npc.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignNpcRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignNpcResponse>(
            ApplicationStatusCode.Success,
            ToResponse(npc));
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

    private async Task<ApplicationStatusCode?> ValidateStoryBlockAndMilestoneAsync(
        Guid campaignId,
        Guid storyBlockId,
        int campaignMilestoneId,
        CancellationToken cancellationToken)
    {
        var storyBlock = await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBlock is null)
        {
            return ApplicationStatusCode.StoryBlockNotFound;
        }

        var milestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            campaignMilestoneId,
            cancellationToken);

        return milestone is null
            ? ApplicationStatusCode.CampaignMilestoneNotFound
            : null;
    }

    private static bool TryBuildStoryBeatInformation(
        CreateInformationStoryBeatRequest? request,
        out StoryBeatInformation information)
    {
        return TryBuildStoryBeatInformation(
            request?.Information,
            out information);
    }

    private static bool TryBuildStoryBeatInformation(
        UpdateInformationStoryBeatRequest? request,
        out StoryBeatInformation information)
    {
        return TryBuildStoryBeatInformation(
            request?.Information,
            out information);
    }

    private static bool TryBuildStoryBeatInformation(
        StoryBeatInformationRequest? request,
        out StoryBeatInformation information)
    {
        information = new StoryBeatInformation();

        var narrative = request?.Narrative?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(narrative)
            || request.OptionalInformation.Any(optional => optional is null))
        {
            return false;
        }

        var optionalInformation = new List<StoryBeatOptionalInformation>();

        foreach (var optionalRequest in request.OptionalInformation)
        {
            var revealedInformation = optionalRequest.Information?.Trim();

            if (!Enum.IsDefined(optionalRequest.Skill)
                || optionalRequest.DifficultyClass is < 1 or > 30
                || string.IsNullOrWhiteSpace(revealedInformation)
                || !Enum.IsDefined(optionalRequest.Placement)
                || !IsValidNarrativeOffset(
                    optionalRequest.Placement,
                    optionalRequest.NarrativeOffset,
                    narrative.Length))
            {
                return false;
            }

            optionalInformation.Add(new StoryBeatOptionalInformation
            {
                Id = Guid.NewGuid(),
                Skill = optionalRequest.Skill,
                DifficultyClass = optionalRequest.DifficultyClass,
                Information = revealedInformation,
                Placement = optionalRequest.Placement,
                NarrativeOffset = optionalRequest.Placement == StoryBeatOptionalInformationPlacement.Inline
                    ? optionalRequest.NarrativeOffset
                    : null
            });
        }

        information = new StoryBeatInformation
        {
            Narrative = narrative,
            OptionalInformation = optionalInformation
        };

        return true;
    }

    private static bool TryBuildStoryBeatNarrative(
        CreateNarrativeStoryBeatRequest? request,
        out StoryBeatNarrative narrative)
    {
        return TryBuildStoryBeatNarrative(
            request?.Narrative,
            out narrative);
    }

    private static bool TryBuildStoryBeatNarrative(
        UpdateNarrativeStoryBeatRequest? request,
        out StoryBeatNarrative narrative)
    {
        return TryBuildStoryBeatNarrative(
            request?.Narrative,
            out narrative);
    }

    private static bool TryBuildStoryBeatNarrative(
        StoryBeatNarrativeRequest? request,
        out StoryBeatNarrative narrative)
    {
        narrative = new StoryBeatNarrative();

        if (request is null
            || request.Paragraphs.Count is 0 or > MaximumNarrativeParagraphCount)
        {
            return false;
        }

        var paragraphs = new List<StoryBeatNarrativeParagraph>();

        for (var index = 0; index < request.Paragraphs.Count; index++)
        {
            var paragraph = request.Paragraphs[index]?.Trim();

            if (string.IsNullOrWhiteSpace(paragraph))
            {
                return false;
            }

            paragraphs.Add(new StoryBeatNarrativeParagraph
            {
                OrderIndex = index + 1,
                Text = paragraph
            });
        }

        narrative = new StoryBeatNarrative
        {
            Paragraphs = paragraphs
        };

        return true;
    }

    private static bool TryBuildStoryBeatDecision(
        CreateDecisionStoryBeatRequest? request,
        StoryBeatDecision? existingDecision,
        out StoryBeatDecision decision)
    {
        return TryBuildStoryBeatDecision(
            request?.Decision,
            existingDecision,
            out decision);
    }

    private static bool TryBuildStoryBeatDecision(
        UpdateDecisionStoryBeatRequest? request,
        StoryBeatDecision? existingDecision,
        out StoryBeatDecision decision)
    {
        return TryBuildStoryBeatDecision(
            request?.Decision,
            existingDecision,
            out decision);
    }

    private static bool TryBuildStoryBeatDecision(
        StoryBeatDecisionRequest? request,
        StoryBeatDecision? existingDecision,
        out StoryBeatDecision decision)
    {
        decision = new StoryBeatDecision();

        var description = request?.Description?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(description)
            || description.Length > MaximumDecisionDescriptionLength
            || request.Decisions.Count is 0 or > MaximumDecisionOptionCount
            || request.Decisions.Any(option => option is null))
        {
            return false;
        }

        var decisions = new List<StoryBeatDecisionOption>();
        var usedDecisionIds = new HashSet<Guid>();

        for (var index = 0; index < request.Decisions.Count; index++)
        {
            var optionRequest = request.Decisions[index];
            var title = optionRequest.Title?.Trim();
            var optionDescription = optionRequest.Description?.Trim();

            if (string.IsNullOrWhiteSpace(title)
                || title.Length > MaximumDecisionOptionTitleLength
                || string.IsNullOrWhiteSpace(optionDescription)
                || optionDescription.Length > MaximumDecisionOptionDescriptionLength)
            {
                return false;
            }

            var decisionId = ResolveDecisionOptionId(
                optionRequest,
                index + 1,
                title,
                optionDescription,
                existingDecision,
                usedDecisionIds);

            if (decisionId == Guid.Empty || !usedDecisionIds.Add(decisionId))
            {
                return false;
            }

            decisions.Add(new StoryBeatDecisionOption
            {
                Id = decisionId,
                OrderIndex = index + 1,
                Title = title,
                Description = optionDescription,
                IsSelected = optionRequest.IsSelected
            });
        }

        decision = new StoryBeatDecision
        {
            Description = description,
            Decisions = decisions
        };

        return true;
    }

    private static Guid ResolveDecisionOptionId(
        StoryBeatDecisionOptionRequest request,
        int orderIndex,
        string title,
        string description,
        StoryBeatDecision? existingDecision,
        IReadOnlySet<Guid> usedDecisionIds)
    {
        if (request.Id is { } requestId && requestId != Guid.Empty)
        {
            return requestId;
        }

        var samePositionId = existingDecision?.Decisions
            .FirstOrDefault(item =>
                item.Id != Guid.Empty
                && !usedDecisionIds.Contains(item.Id)
                && item.OrderIndex == orderIndex)
            ?.Id;

        if (samePositionId is { } positionId)
        {
            return positionId;
        }

        return existingDecision?.Decisions
            .FirstOrDefault(item =>
                item.Id != Guid.Empty
                && !usedDecisionIds.Contains(item.Id)
                && string.Equals(item.Title, title, StringComparison.Ordinal)
                && string.Equals(item.Description, description, StringComparison.Ordinal))
            ?.Id ?? Guid.NewGuid();
    }

    private static bool TryBuildStoryBeatCombat(
        CreateCombatStoryBeatRequest? request,
        out StoryBeatCombat combat,
        out IReadOnlyCollection<int> monsterIds)
    {
        return TryBuildStoryBeatCombat(
            request?.Combat,
            out combat,
            out monsterIds);
    }

    private static bool TryBuildStoryBeatCombat(
        UpdateCombatStoryBeatRequest? request,
        out StoryBeatCombat combat,
        out IReadOnlyCollection<int> monsterIds)
    {
        return TryBuildStoryBeatCombat(
            request?.Combat,
            out combat,
            out monsterIds);
    }

    private static bool TryBuildStoryBeatCombat(
        StoryBeatCombatRequest? request,
        out StoryBeatCombat combat,
        out IReadOnlyCollection<int> monsterIds)
    {
        combat = new StoryBeatCombat();
        monsterIds = [];

        var description = request?.Description?.Trim();
        var rewards = string.IsNullOrWhiteSpace(request?.Rewards)
            ? null
            : request.Rewards.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(description)
            || description.Length > MaximumCombatDescriptionLength
            || rewards is not null && rewards.Length > MaximumCombatRewardsLength
            || request.EnemyNpcs.Count is 0 or > MaximumCombatEnemyNpcCount
            || request.EnemyNpcs.Any(enemyNpc => enemyNpc is null))
        {
            return false;
        }

        var enemyNpcs = new List<StoryBeatCombatEnemyNpc>();
        var monsterIdSet = new HashSet<int>();

        foreach (var enemyNpcRequest in request.EnemyNpcs)
        {
            if (enemyNpcRequest.MonsterId < 1
                || enemyNpcRequest.Amount < 1
                || !monsterIdSet.Add(enemyNpcRequest.MonsterId))
            {
                return false;
            }

            enemyNpcs.Add(new StoryBeatCombatEnemyNpc
            {
                MonsterId = enemyNpcRequest.MonsterId,
                Amount = enemyNpcRequest.Amount
            });
        }

        combat = new StoryBeatCombat
        {
            Description = description,
            Rewards = rewards,
            EnemyNpcs = enemyNpcs
        };
        monsterIds = monsterIdSet;

        return true;
    }

    private static bool TryBuildStoryBeatTransition(
        CreateTransitionStoryBeatRequest? request,
        out StoryBeatTransition transition)
    {
        return TryBuildStoryBeatTransition(
            request?.Transition,
            out transition);
    }

    private static bool TryBuildStoryBeatTransition(
        UpdateTransitionStoryBeatRequest? request,
        out StoryBeatTransition transition)
    {
        return TryBuildStoryBeatTransition(
            request?.Transition,
            out transition);
    }

    private static bool TryBuildStoryBeatTransition(
        StoryBeatTransitionRequest? request,
        out StoryBeatTransition transition)
    {
        transition = new StoryBeatTransition();

        var description = request?.Description?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(description)
            || description.Length > MaximumTransitionDescriptionLength)
        {
            return false;
        }

        transition = new StoryBeatTransition
        {
            Description = description
        };

        return true;
    }

    private async Task<bool> AllMonstersAreEnabledForCampaignAsync(
        Guid campaignId,
        IReadOnlyCollection<int> monsterIds,
        CancellationToken cancellationToken)
    {
        if (monsterIds.Count == 0)
        {
            return false;
        }

        var enabledMonsterCount = await _monsterRepository.CountCampaignParticipationsByMonsterIdsAsync(
            campaignId,
            monsterIds,
            cancellationToken);

        return enabledMonsterCount == monsterIds.Count;
    }

    private Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        CreateRoleplayingStoryBeatRequest? request,
        StoryBeatRoleplaying? existingRoleplaying,
        CancellationToken cancellationToken)
    {
        return BuildStoryBeatRoleplayingAsync(
            campaignId,
            request?.Roleplaying,
            existingRoleplaying,
            cancellationToken);
    }

    private Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        UpdateRoleplayingStoryBeatRequest? request,
        StoryBeatRoleplaying? existingRoleplaying,
        CancellationToken cancellationToken)
    {
        return BuildStoryBeatRoleplayingAsync(
            campaignId,
            request?.Roleplaying,
            existingRoleplaying,
            cancellationToken);
    }

    private async Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        StoryBeatRoleplayingRequest? request,
        StoryBeatRoleplaying? existingRoleplaying,
        CancellationToken cancellationToken)
    {
        var mainDescription = request?.MainDescription?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(mainDescription)
            || request.NpcTags.Count is 0
            || request.DiscoverableInformation.Any(information => information is null))
        {
            return null;
        }

        var npcTags = request.NpcTags
            .Select(NormalizeCampaignNpcTag)
            .ToList();

        if (npcTags.Any(string.IsNullOrWhiteSpace)
            || npcTags.Distinct(StringComparer.OrdinalIgnoreCase).Count() != npcTags.Count)
        {
            return null;
        }

        var existingNpcs = await _campaignNpcRepository.ListByCampaignIdAndTagsAsync(
            campaignId,
            npcTags!,
            cancellationToken);

        if (existingNpcs.Count != npcTags.Count)
        {
            return null;
        }

        var npcTagSet = new HashSet<string>(npcTags!, StringComparer.OrdinalIgnoreCase);
        var discoverableInformation = new List<StoryBeatRoleplayingInformation>();
        var usedInformationIds = new HashSet<Guid>();

        foreach (var informationRequest in request.DiscoverableInformation)
        {
            var npcTag = NormalizeCampaignNpcTag(informationRequest.NpcTag);
            var information = informationRequest.Information?.Trim();

            if (string.IsNullOrWhiteSpace(npcTag)
                || !npcTagSet.Contains(npcTag)
                || string.IsNullOrWhiteSpace(information)
                || !IsValidRoleplayingCheck(informationRequest))
            {
                return null;
            }

            var informationId = ResolveRoleplayingInformationId(
                informationRequest,
                npcTag,
                information,
                existingRoleplaying);

            if (informationId == Guid.Empty || !usedInformationIds.Add(informationId))
            {
                return null;
            }

            discoverableInformation.Add(new StoryBeatRoleplayingInformation
            {
                Id = informationId,
                NpcTag = npcTag,
                CheckType = informationRequest.CheckType,
                Skill = informationRequest.CheckType == StoryBeatRoleplayingCheckType.Skill
                    ? informationRequest.Skill
                    : null,
                Ability = informationRequest.CheckType == StoryBeatRoleplayingCheckType.Ability
                    ? informationRequest.Ability
                    : null,
                DifficultyClass = informationRequest.CheckType == StoryBeatRoleplayingCheckType.None
                    ? null
                    : informationRequest.DifficultyClass,
                Information = information
            });
        }

        return new StoryBeatRoleplaying
        {
            MainDescription = mainDescription!,
            NpcReferences = npcTags!
                .Select(tag => new StoryBeatRoleplayingNpcReference
                {
                    Id = ResolveRoleplayingNpcReferenceId(tag!, existingRoleplaying),
                    NpcTag = tag!
                })
                .ToList(),
            DiscoverableInformation = discoverableInformation
        };
    }

    private static Guid ResolveRoleplayingNpcReferenceId(
        string npcTag,
        StoryBeatRoleplaying? existingRoleplaying)
    {
        return existingRoleplaying?.NpcReferences
            .FirstOrDefault(item => string.Equals(
                item.NpcTag,
                npcTag,
                StringComparison.OrdinalIgnoreCase))
            ?.Id is { } existingId && existingId != Guid.Empty
                ? existingId
                : Guid.NewGuid();
    }

    private static Guid ResolveRoleplayingInformationId(
        StoryBeatRoleplayingInformationRequest request,
        string npcTag,
        string information,
        StoryBeatRoleplaying? existingRoleplaying)
    {
        if (request.Id is { } requestId && requestId != Guid.Empty)
        {
            return requestId;
        }

        return existingRoleplaying?.DiscoverableInformation
            .FirstOrDefault(item =>
                item.Id != Guid.Empty
                && string.Equals(item.NpcTag, npcTag, StringComparison.OrdinalIgnoreCase)
                && item.CheckType == request.CheckType
                && item.Skill == request.Skill
                && item.Ability == request.Ability
                && item.DifficultyClass == request.DifficultyClass
                && string.Equals(item.Information, information, StringComparison.Ordinal))
            ?.Id ?? Guid.NewGuid();
    }

    private static bool IsValidRoleplayingCheck(StoryBeatRoleplayingInformationRequest request)
    {
        return request.CheckType switch
        {
            StoryBeatRoleplayingCheckType.None => request.Skill is null
                && request.Ability is null
                && request.DifficultyClass is null,
            StoryBeatRoleplayingCheckType.Skill => request.Skill.HasValue
                && Enum.IsDefined(request.Skill.Value)
                && request.Ability is null
                && request.DifficultyClass is >= 1 and <= 30,
            StoryBeatRoleplayingCheckType.Ability => request.Ability.HasValue
                && Enum.IsDefined(request.Ability.Value)
                && request.Skill is null
                && request.DifficultyClass is >= 1 and <= 30,
            _ => false
        };
    }

    private static bool IsValidNarrativeOffset(
        StoryBeatOptionalInformationPlacement placement,
        int? narrativeOffset,
        int narrativeLength)
    {
        return placement switch
        {
            StoryBeatOptionalInformationPlacement.Appended => true,
            StoryBeatOptionalInformationPlacement.Inline => narrativeOffset >= 0
                && narrativeOffset <= narrativeLength,
            _ => false
        };
    }

    private static bool IsValidStoryBlockTitle(string? title)
    {
        return !string.IsNullOrWhiteSpace(title)
            && title.Length <= MaximumStoryBlockTitleLength;
    }

    private static bool IsValidStoryBeatTitle(string? title)
    {
        return !string.IsNullOrWhiteSpace(title)
            && title.Length <= MaximumStoryBeatTitleLength;
    }

    private static string? NormalizeCampaignNpcTag(string? tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? null
            : tag.Trim().ToLowerInvariant();
    }

    private static string? ResolveCampaignNpcDisplayName(
        string? displayName,
        string? nickname,
        string? legacyName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(nickname))
        {
            return nickname.Trim();
        }

        return string.IsNullOrWhiteSpace(legacyName)
            ? null
            : legacyName.Trim();
    }

    private static bool IsValidCampaignNpc(
        string? tag,
        string? name,
        string? displayName,
        string description)
    {
        return !string.IsNullOrWhiteSpace(tag)
            && tag.Length <= MaximumCampaignNpcTagLength
            && !string.IsNullOrWhiteSpace(name)
            && name.Length <= MaximumCampaignNpcNameLength
            && !string.IsNullOrWhiteSpace(displayName)
            && displayName.Length <= MaximumCampaignNpcDisplayNameLength
            && description.Length <= MaximumCampaignNpcDescriptionLength;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidReorderStoryBeatsRequest(ReorderStoryBeatsRequest? request)
    {
        if (request?.StoryBeats is null
            || request.StoryBeats.Count == 0
            || request.StoryBeats
                .Select(beat => beat.StoryBeatId)
                .Distinct()
                .Count() != request.StoryBeats.Count)
        {
            return false;
        }

        if (request.StoryBeats.Any(beat =>
                beat.StoryBeatId == Guid.Empty
                || beat.OrderIndex < 1
                || beat.SecondaryOrderIndex < 1))
        {
            return false;
        }

        return request.StoryBeats
            .Select(beat => (beat.OrderIndex, beat.SecondaryOrderIndex))
            .Distinct()
            .Count() == request.StoryBeats.Count;
    }

    private static bool TransitionWouldRemainFinal(
        IReadOnlyList<StoryBeat> storyBeats,
        IReadOnlyList<ReorderStoryBeatRequest> requestedStoryBeats)
    {
        var transitionBeat = storyBeats.SingleOrDefault(
            beat => beat.StoryBeatType == StoryBeatType.Transition);

        if (transitionBeat is null)
        {
            return true;
        }

        var positionsByStoryBeatId = requestedStoryBeats.ToDictionary(
            beat => beat.StoryBeatId);
        var transitionPosition = positionsByStoryBeatId[transitionBeat.Id];

        return requestedStoryBeats.All(beat =>
            beat.StoryBeatId == transitionBeat.Id
            || beat.OrderIndex < transitionPosition.OrderIndex
            || beat.OrderIndex == transitionPosition.OrderIndex
                && beat.SecondaryOrderIndex < transitionPosition.SecondaryOrderIndex);
    }

    private async Task ApplyStoryBeatOrderAsync(
        IReadOnlyList<StoryBeat> storyBeats,
        IReadOnlyList<ReorderStoryBeatRequest> requestedStoryBeats,
        CancellationToken cancellationToken)
    {
        var beatsById = storyBeats.ToDictionary(beat => beat.Id);

        for (var index = 0; index < storyBeats.Count; index++)
        {
            storyBeats[index].OrderIndex = -(index + 1);
            storyBeats[index].SecondaryOrderIndex = 1;
        }

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        foreach (var requestedStoryBeat in requestedStoryBeats)
        {
            var storyBeat = beatsById[requestedStoryBeat.StoryBeatId];

            storyBeat.OrderIndex = requestedStoryBeat.OrderIndex;
            storyBeat.SecondaryOrderIndex = requestedStoryBeat.SecondaryOrderIndex;
        }

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyStoryBlockOrderAsync(
        IReadOnlyList<StoryBlock> storyBlocks,
        IReadOnlyList<Guid> orderedStoryBlockIds,
        CancellationToken cancellationToken)
    {
        var blocksById = storyBlocks.ToDictionary(block => block.StoryBlockId);

        for (var index = 0; index < storyBlocks.Count; index++)
        {
            storyBlocks[index].OrderIndex = -(index + 1);
        }

        await _storyBlockRepository.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < orderedStoryBlockIds.Count; index++)
        {
            blocksById[orderedStoryBlockIds[index]].OrderIndex = index + 1;
        }

        await _storyBlockRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task CompactStoryBlockOrderAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var storyBlocks = await _storyBlockRepository.ListTrackedByCampaignIdAsync(
            campaignId,
            cancellationToken);

        if (storyBlocks.Count == 0)
        {
            return;
        }

        var orderedStoryBlockIds = storyBlocks
            .OrderBy(block => block.OrderIndex)
            .Select(block => block.StoryBlockId)
            .ToList();

        await ApplyStoryBlockOrderAsync(
            storyBlocks,
            orderedStoryBlockIds,
            cancellationToken);
    }

    private static StoryBlockResponse ToResponse(StoryBlock storyBlock)
    {
        return new StoryBlockResponse
        {
            StoryBlockId = storyBlock.StoryBlockId,
            CampaignId = storyBlock.CampaignId,
            Title = storyBlock.Title,
            OrderIndex = storyBlock.OrderIndex
        };
    }

    private async Task<IReadOnlyDictionary<int, StoryBeatIndexPathRuleResponse>>
        GetStoryBeatIndexPathRulesByOrderIndexAsync(
            Guid campaignId,
            Guid storyBlockId,
            CancellationToken cancellationToken)
    {
        var indexPathRules = await _storyBeatIndexPathRuleRepository.ListByStoryBlockAsync(
            campaignId,
            storyBlockId,
            cancellationToken);

        return indexPathRules
            .GroupBy(rule => rule.OrderIndex)
            .ToDictionary(
                group => group.Key,
                group => ToResponse(group.First()));
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

    private static StoryBlockMilestoneResponse ToResponse(StoryBlockMilestone link)
    {
        return new StoryBlockMilestoneResponse
        {
            StoryBlockId = link.StoryBlockId,
            CampaignMilestoneId = link.CampaignMilestoneId,
            OrderIndex = link.OrderIndex,
            Milestone = ToResponse(link.CampaignMilestone)
        };
    }

    private static CampaignNpcResponse ToResponse(CampaignNpc npc)
    {
        return new CampaignNpcResponse
        {
            CampaignNpcId = npc.CampaignNpcId,
            CampaignId = npc.CampaignId,
            Tag = npc.Tag,
            Name = npc.Name,
            DisplayName = string.IsNullOrWhiteSpace(npc.DisplayName)
                ? npc.Name
                : npc.DisplayName,
            Description = npc.Description,
            CreatedAt = npc.CreatedAt,
            UpdatedAt = npc.UpdatedAt
        };
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

    private static StoryBeatResponse ToResponse(
        StoryBeat storyBeat,
        IReadOnlyList<StoryBeat>? storyBlockBeats = null,
        IReadOnlyDictionary<int, StoryBeatIndexPathRuleResponse>? indexPathRulesByOrderIndex = null)
    {
        var indexPathRule = indexPathRulesByOrderIndex?.GetValueOrDefault(storyBeat.OrderIndex);

        return new StoryBeatResponse
        {
            StoryBeatId = storyBeat.Id,
            StoryBlockId = storyBeat.StoryBlockId,
            OrderIndex = storyBeat.OrderIndex,
            SecondaryOrderIndex = storyBeat.SecondaryOrderIndex,
            Title = storyBeat.Title,
            StoryBeatType = storyBeat.StoryBeatType,
            Information = storyBeat.Information is null
                ? null
                : ToResponse(storyBeat.Information),
            Narrative = storyBeat.Narrative is null
                ? null
                : ToResponse(storyBeat.Narrative),
            Roleplaying = storyBeat.Roleplaying is null
                ? null
                : ToResponse(storyBeat.Roleplaying),
            Decision = storyBeat.Decision is null
                ? null
                : ToResponse(storyBeat.Decision, storyBeat.Id),
            Combat = storyBeat.Combat is null
                ? null
                : ToResponse(storyBeat.Combat),
            Transition = storyBeat.Transition is null
                ? null
                : ToResponse(storyBeat.Transition, storyBeat, storyBlockBeats ?? []),
            Milestone = storyBeat.Milestone is null
                ? null
                : ToResponse(storyBeat.Milestone),
            IndexPathRule = indexPathRule
        };
    }

    private static StoryBeatInformationResponse ToResponse(StoryBeatInformation information)
    {
        return new StoryBeatInformationResponse
        {
            Narrative = information.Narrative,
            OptionalInformation = information.OptionalInformation
                .Select(ToResponse)
                .ToList()
        };
    }

    private static StoryBeatOptionalInformationResponse ToResponse(
        StoryBeatOptionalInformation optionalInformation)
    {
        return new StoryBeatOptionalInformationResponse
        {
            Id = optionalInformation.Id,
            Skill = optionalInformation.Skill,
            DifficultyClass = optionalInformation.DifficultyClass,
            Information = optionalInformation.Information,
            Placement = optionalInformation.Placement,
            NarrativeOffset = optionalInformation.NarrativeOffset
        };
    }

    private static StoryBeatRoleplayingResponse ToResponse(StoryBeatRoleplaying roleplaying)
    {
        return new StoryBeatRoleplayingResponse
        {
            MainDescription = roleplaying.MainDescription,
            NpcTags = roleplaying.NpcReferences
                .Select(npc => npc.NpcTag)
                .ToList(),
            NpcReferences = roleplaying.NpcReferences
                .Select(npc => new StoryBeatRoleplayingNpcReferenceResponse
                {
                    Id = npc.Id,
                    NpcTag = npc.NpcTag
                })
                .ToList(),
            DiscoverableInformation = roleplaying.DiscoverableInformation
                .Select(ToResponse)
                .ToList()
        };
    }

    private static StoryBeatRoleplayingInformationResponse ToResponse(
        StoryBeatRoleplayingInformation information)
    {
        return new StoryBeatRoleplayingInformationResponse
        {
            Id = information.Id,
            NpcTag = string.IsNullOrWhiteSpace(information.NpcTag)
                ? string.Empty
                : information.NpcTag,
            CheckType = information.CheckType,
            Skill = information.Skill,
            Ability = information.Ability,
            DifficultyClass = information.DifficultyClass,
            Information = information.Information
        };
    }

    private static StoryBeatDecisionResponse ToResponse(
        StoryBeatDecision decision,
        Guid storyBeatId)
    {
        return new StoryBeatDecisionResponse
        {
            Description = decision.Description,
            Decisions = decision.Decisions
                .OrderBy(option => option.OrderIndex)
                .Select(option => ToResponse(option, storyBeatId))
                .ToList()
        };
    }

    private static StoryBeatDecisionOptionResponse ToResponse(
        StoryBeatDecisionOption decision,
        Guid storyBeatId)
    {
        return new StoryBeatDecisionOptionResponse
        {
            Id = ResolveDecisionOptionResponseId(decision, storyBeatId),
            OrderIndex = decision.OrderIndex,
            Title = decision.Title,
            Description = decision.Description,
            IsSelected = decision.IsSelected
        };
    }

    private static Guid ResolveDecisionOptionResponseId(
        StoryBeatDecisionOption decision,
        Guid storyBeatId)
    {
        return decision.Id != Guid.Empty
            ? decision.Id
            : CreateDeterministicDecisionOptionId(storyBeatId, decision.OrderIndex);
    }

    private static Guid CreateDeterministicDecisionOptionId(
        Guid storyBeatId,
        int orderIndex)
    {
        var input = $"{storyBeatId:N}:decision:{orderIndex}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, guidBytes.Length);

        return new Guid(guidBytes);
    }

    private static StoryBeatCombatResponse ToResponse(StoryBeatCombat combat)
    {
        return new StoryBeatCombatResponse
        {
            Description = combat.Description,
            Rewards = combat.Rewards,
            EnemyNpcs = combat.EnemyNpcs
                .Select(ToResponse)
                .ToList()
        };
    }

    private static StoryBeatCombatEnemyNpcResponse ToResponse(StoryBeatCombatEnemyNpc enemyNpc)
    {
        return new StoryBeatCombatEnemyNpcResponse
        {
            MonsterId = enemyNpc.MonsterId,
            Amount = enemyNpc.Amount
        };
    }

    private static StoryBeatTransitionResponse ToResponse(
        StoryBeatTransition transition,
        StoryBeat transitionBeat,
        IReadOnlyList<StoryBeat> storyBlockBeats)
    {
        return new StoryBeatTransitionResponse
        {
            Description = transition.Description,
            Conclusions = BuildTransitionConclusions(transitionBeat, storyBlockBeats)
        };
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse> BuildTransitionConclusions(
        StoryBeat transitionBeat,
        IReadOnlyList<StoryBeat> storyBlockBeats)
    {
        return storyBlockBeats
            .Where(beat => beat.StoryBlockId == transitionBeat.StoryBlockId
                && IsBefore(beat, transitionBeat))
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.Id)
            .SelectMany(BuildTransitionConclusions)
            .ToList();
    }

    private static bool IsBefore(
        StoryBeat beat,
        StoryBeat comparisonBeat)
    {
        return beat.OrderIndex < comparisonBeat.OrderIndex
            || (beat.OrderIndex == comparisonBeat.OrderIndex
                && beat.SecondaryOrderIndex < comparisonBeat.SecondaryOrderIndex);
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse> BuildTransitionConclusions(
        StoryBeat storyBeat)
    {
        return storyBeat.StoryBeatType switch
        {
            StoryBeatType.Information => BuildInformationTransitionConclusions(storyBeat),
            StoryBeatType.Narrative => BuildNarrativeTransitionConclusions(storyBeat),
            StoryBeatType.Roleplaying => BuildRoleplayingTransitionConclusions(storyBeat),
            StoryBeatType.Decision => BuildDecisionTransitionConclusions(storyBeat),
            StoryBeatType.Combat => BuildCombatTransitionConclusions(storyBeat),
            StoryBeatType.Milestone => BuildMilestoneTransitionConclusions(storyBeat),
            _ => []
        };
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildInformationTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Information is null)
        {
            return [];
        }

        var conclusions = new List<StoryBeatTransitionConclusionResponse>
        {
            CreateTransitionConclusion(
                storyBeat,
                "Information Shown",
                storyBeat.Information.Narrative)
        };

        conclusions.AddRange(storyBeat.Information.OptionalInformation
            .OrderBy(information => information.Placement)
            .ThenBy(information => information.NarrativeOffset ?? int.MaxValue)
            .Select(information => CreateTransitionConclusion(
                storyBeat,
                "Passive Check",
                $"{information.Skill}-{information.DifficultyClass}: {information.Information}")));

        return conclusions;
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildNarrativeTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Narrative is null)
        {
            return [];
        }

        return storyBeat.Narrative.Paragraphs
            .OrderBy(paragraph => paragraph.OrderIndex)
            .Select(paragraph => CreateTransitionConclusion(
                storyBeat,
                "Narrative",
                paragraph.Text))
            .ToList();
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildRoleplayingTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Roleplaying is null)
        {
            return [];
        }

        var npcConclusions = storyBeat.Roleplaying.NpcReferences
            .Select(npc => CreateTransitionConclusion(
                storyBeat,
                "NPC Talked",
                npc.NpcTag))
            .ToList();

        var informationConclusions = storyBeat.Roleplaying.DiscoverableInformation
            .Select(information => CreateTransitionConclusion(
                storyBeat,
                "Roleplaying Check",
                $"{RoleplayingCheckLabel(information)}: {information.Information}"));

        npcConclusions.AddRange(informationConclusions);

        return npcConclusions;
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildDecisionTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Decision is null)
        {
            return [];
        }

        return storyBeat.Decision.Decisions
            .OrderBy(decision => decision.OrderIndex)
            .Select(decision => CreateTransitionConclusion(
                storyBeat,
                decision.IsSelected ? "Decision Taken" : "Decision Shown",
                $"{decision.Title}: {decision.Description}"))
            .ToList();
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildCombatTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Combat is null)
        {
            return [];
        }

        var conclusions = new List<StoryBeatTransitionConclusionResponse>
        {
            CreateTransitionConclusion(
                storyBeat,
                "Encounter",
                storyBeat.Combat.Description)
        };

        conclusions.AddRange(storyBeat.Combat.EnemyNpcs
            .Select(enemyNpc => CreateTransitionConclusion(
                storyBeat,
                "Encounter Enemy",
                $"{enemyNpc.Amount} x Monster {enemyNpc.MonsterId}")));

        if (!string.IsNullOrWhiteSpace(storyBeat.Combat.Rewards))
        {
            conclusions.Add(CreateTransitionConclusion(
                storyBeat,
                "Combat Reward",
                storyBeat.Combat.Rewards));
        }

        return conclusions;
    }

    private static IReadOnlyList<StoryBeatTransitionConclusionResponse>
        BuildMilestoneTransitionConclusions(StoryBeat storyBeat)
    {
        if (storyBeat.Milestone is null)
        {
            return [];
        }

        return
        [
            CreateTransitionConclusion(
                storyBeat,
                "Milestone",
                $"{storyBeat.Milestone.Title}: {storyBeat.Milestone.Description ?? string.Empty}".Trim())
        ];
    }

    private static StoryBeatTransitionConclusionResponse CreateTransitionConclusion(
        StoryBeat storyBeat,
        string category,
        string text)
    {
        return new StoryBeatTransitionConclusionResponse
        {
            SourceStoryBeatId = storyBeat.Id,
            SourceTitle = storyBeat.Title,
            SourceStoryBeatType = storyBeat.StoryBeatType,
            Category = category,
            Text = text
        };
    }

    private static string RoleplayingCheckLabel(StoryBeatRoleplayingInformation information)
    {
        return information.CheckType switch
        {
            StoryBeatRoleplayingCheckType.None => string.IsNullOrWhiteSpace(information.NpcTag)
                ? "No check"
                : $"{information.NpcTag} - No check",
            StoryBeatRoleplayingCheckType.Skill => $"{information.NpcTag} - {information.Skill}-{information.DifficultyClass}",
            StoryBeatRoleplayingCheckType.Ability => $"{information.NpcTag} - {information.Ability}-{information.DifficultyClass}",
            _ => information.NpcTag
        };
    }

    private static StoryBeatNarrativeResponse ToResponse(StoryBeatNarrative narrative)
    {
        return new StoryBeatNarrativeResponse
        {
            Paragraphs = narrative.Paragraphs
                .OrderBy(paragraph => paragraph.OrderIndex)
                .Select(paragraph => paragraph.Text)
                .ToList()
        };
    }
}
