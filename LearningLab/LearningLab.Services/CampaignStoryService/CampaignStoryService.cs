using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Data.Repositories.CampaignMilestoneRepository;
using LearningLab.Data.Repositories.CampaignNpcRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.StoryBlockMilestoneRepository;
using LearningLab.Data.Repositories.StoryBlockRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignStoryService;

public sealed class CampaignStoryService : ICampaignStoryService
{
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

    private readonly IStoryBlockRepository _storyBlockRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IStoryBlockMilestoneRepository _storyBlockMilestoneRepository;
    private readonly ICampaignMilestoneRepository _campaignMilestoneRepository;
    private readonly ICampaignNpcRepository _campaignNpcRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public CampaignStoryService(
        IStoryBlockRepository storyBlockRepository,
        IStoryBeatRepository storyBeatRepository,
        IStoryBlockMilestoneRepository storyBlockMilestoneRepository,
        ICampaignMilestoneRepository campaignMilestoneRepository,
        ICampaignNpcRepository campaignNpcRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _storyBlockRepository = storyBlockRepository;
        _storyBeatRepository = storyBeatRepository;
        _storyBlockMilestoneRepository = storyBlockMilestoneRepository;
        _campaignMilestoneRepository = campaignMilestoneRepository;
        _campaignNpcRepository = campaignNpcRepository;
        _campaignRepository = campaignRepository;
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

        var storyBlock = new StoryBlock
        {
            StoryBlockId = Guid.NewGuid(),
            CampaignId = campaignId,
            Title = title!
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

        if (!TryBuildStoryBeatInformation(request, out var information))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = (latestOrderIndex ?? 0) + 1,
            Title = title!,
            StoryBeatType = StoryBeatType.Information,
            Information = information,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
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

        if (!TryBuildStoryBeatNarrative(request, out var narrative))
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = (latestOrderIndex ?? 0) + 1,
            Title = title!,
            StoryBeatType = StoryBeatType.Narrative,
            Information = null,
            Narrative = narrative,
            Roleplaying = null,
            Decision = null,
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

        var roleplaying = await BuildStoryBeatRoleplayingAsync(
            campaignId,
            request,
            cancellationToken);

        if (roleplaying is null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.InvalidStoryBeat);
        }

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = (latestOrderIndex ?? 0) + 1,
            Title = title!,
            StoryBeatType = StoryBeatType.Roleplaying,
            Information = null,
            Narrative = null,
            Roleplaying = roleplaying,
            Decision = null,
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
            || !TryBuildStoryBeatDecision(request, out var decision))
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

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = (latestOrderIndex ?? 0) + 1,
            Title = title!,
            StoryBeatType = StoryBeatType.Decision,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = decision,
            CampaignMilestoneId = null,
            Milestone = null
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
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

        if (existingMilestoneBeat is not null)
        {
            return new ServiceResult<StoryBeatResponse>(ApplicationStatusCode.StoryBeatMilestoneAlreadyExists);
        }

        var latestOrderIndex = await _storyBeatRepository.GetLatestOrderIndexByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);

        var storyBeat = new StoryBeat
        {
            Id = Guid.NewGuid(),
            StoryBlockId = storyBlockId,
            OrderIndex = (latestOrderIndex ?? 0) + 1,
            Title = title!,
            StoryBeatType = StoryBeatType.Milestone,
            Information = null,
            Narrative = null,
            Roleplaying = null,
            Decision = null,
            CampaignMilestoneId = request.MilestoneId,
            Milestone = milestone
        };

        await _storyBeatRepository.AddAsync(storyBeat, cancellationToken);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
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

        if (!IsValidStoryBeatTitle(title)
            || !TryBuildStoryBeatDecision(request, out var decision))
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

        storyBeat.Title = title!;
        storyBeat.Information = null;
        storyBeat.Narrative = null;
        storyBeat.Roleplaying = null;
        storyBeat.Decision = decision;
        storyBeat.CampaignMilestoneId = null;
        storyBeat.Milestone = null;

        await _storyBeatRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatResponse>(
            ApplicationStatusCode.Success,
            ToResponse(storyBeat));
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

        _storyBeatRepository.Remove(storyBeat);
        await _storyBeatRepository.SaveChangesAsync(cancellationToken);
        await _storyBeatRepository.DecrementOrderAfterAsync(
            storyBlockId,
            removedOrderIndex,
            cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
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

        return new ServiceResult<IReadOnlyList<StoryBeatResponse>>(
            ApplicationStatusCode.Success,
            storyBeats.Select(ToResponse).ToList());
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
        out StoryBeatDecision decision)
    {
        return TryBuildStoryBeatDecision(
            request?.Decision,
            out decision);
    }

    private static bool TryBuildStoryBeatDecision(
        UpdateDecisionStoryBeatRequest? request,
        out StoryBeatDecision decision)
    {
        return TryBuildStoryBeatDecision(
            request?.Decision,
            out decision);
    }

    private static bool TryBuildStoryBeatDecision(
        StoryBeatDecisionRequest? request,
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

            decisions.Add(new StoryBeatDecisionOption
            {
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

    private Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        CreateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken)
    {
        return BuildStoryBeatRoleplayingAsync(
            campaignId,
            request?.Roleplaying,
            cancellationToken);
    }

    private Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        UpdateRoleplayingStoryBeatRequest? request,
        CancellationToken cancellationToken)
    {
        return BuildStoryBeatRoleplayingAsync(
            campaignId,
            request?.Roleplaying,
            cancellationToken);
    }

    private async Task<StoryBeatRoleplaying?> BuildStoryBeatRoleplayingAsync(
        Guid campaignId,
        StoryBeatRoleplayingRequest? request,
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

            discoverableInformation.Add(new StoryBeatRoleplayingInformation
            {
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
                    NpcTag = tag!
                })
                .ToList(),
            DiscoverableInformation = discoverableInformation
        };
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

    private static StoryBlockResponse ToResponse(StoryBlock storyBlock)
    {
        return new StoryBlockResponse
        {
            StoryBlockId = storyBlock.StoryBlockId,
            CampaignId = storyBlock.CampaignId,
            Title = storyBlock.Title
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

    private static StoryBeatResponse ToResponse(StoryBeat storyBeat)
    {
        return new StoryBeatResponse
        {
            StoryBeatId = storyBeat.Id,
            StoryBlockId = storyBeat.StoryBlockId,
            OrderIndex = storyBeat.OrderIndex,
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
                : ToResponse(storyBeat.Decision),
            Milestone = storyBeat.Milestone is null
                ? null
                : ToResponse(storyBeat.Milestone)
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

    private static StoryBeatDecisionResponse ToResponse(StoryBeatDecision decision)
    {
        return new StoryBeatDecisionResponse
        {
            Description = decision.Description,
            Decisions = decision.Decisions
                .OrderBy(option => option.OrderIndex)
                .Select(ToResponse)
                .ToList()
        };
    }

    private static StoryBeatDecisionOptionResponse ToResponse(StoryBeatDecisionOption decision)
    {
        return new StoryBeatDecisionOptionResponse
        {
            OrderIndex = decision.OrderIndex,
            Title = decision.Title,
            Description = decision.Description,
            IsSelected = decision.IsSelected
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
