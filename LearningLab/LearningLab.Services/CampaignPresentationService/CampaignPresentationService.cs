using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Presentation;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Data.Repositories.CampaignPresentationRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSessionRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.StoryBlockRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignPresentationService;

public sealed class CampaignPresentationService : ICampaignPresentationService
{
    private readonly ICampaignPresentationRepository _campaignPresentationRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignSessionRepository _campaignSessionRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IStoryBlockRepository _storyBlockRepository;
    private readonly IUserRepository _userRepository;

    public CampaignPresentationService(
        ICampaignPresentationRepository campaignPresentationRepository,
        ICampaignRepository campaignRepository,
        ICampaignSessionRepository campaignSessionRepository,
        IStoryBeatRepository storyBeatRepository,
        IStoryBlockRepository storyBlockRepository,
        IUserRepository userRepository)
    {
        _campaignPresentationRepository = campaignPresentationRepository;
        _campaignRepository = campaignRepository;
        _campaignSessionRepository = campaignSessionRepository;
        _storyBeatRepository = storyBeatRepository;
        _storyBlockRepository = storyBlockRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<CampaignPresentationResponse>> GetPresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                validationStatusCode.Value);
        }

        if (!await CampaignSessionExistsAsync(campaignId, sessionId, cancellationToken))
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var presentation = await _campaignPresentationRepository.GetByCampaignSessionIdAsync(
            sessionId,
            cancellationToken);

        if (presentation is null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignPresentationNotFound);
        }

        return await BuildPresentationResponseAsync(presentation, cancellationToken);
    }

    public async Task<ServiceResult<CampaignPresentationResponse>> InitiatePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                validationStatusCode.Value);
        }

        if (!await CampaignSessionExistsAsync(campaignId, sessionId, cancellationToken))
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var presentation = await _campaignPresentationRepository.GetByCampaignSessionIdAsync(
            sessionId,
            cancellationToken);
        var latestEntry = presentation is null
            ? null
            : await _campaignPresentationRepository.GetLatestEntryAsync(
                presentation.Id,
                cancellationToken);
        var selectedStoryBlock = await ResolveStoryBlockAsync(
            campaignId,
            request?.StoryBlockId,
            presentation,
            latestEntry,
            cancellationToken);

        if (selectedStoryBlock is null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var currentStoryBeat = await ResolveStoryBeatAsync(
            campaignId,
            selectedStoryBlock.StoryBlockId,
            latestEntry,
            request?.StoryBlockId is not null,
            cancellationToken);
        var updatedAt = DateTimeOffset.UtcNow;
        var shouldAppendEntry = presentation is null
            || latestEntry is null
            || request?.StoryBlockId is not null
                && latestEntry.StoryBlockId != selectedStoryBlock.StoryBlockId;

        if (presentation is null)
        {
            presentation = new CampaignPresentation
            {
                CampaignSessionId = sessionId,
                Status = PresentationStatus.Active,
                StartedAt = updatedAt,
                UpdatedAt = updatedAt
            };

            await _campaignPresentationRepository.AddAsync(
                presentation,
                cancellationToken);
        }

        if (currentStoryBeat is not null
            && !await TryApplyStoryBeatSelectionAsync(
                presentation,
                currentStoryBeat,
                updatedAt,
                cancellationToken))
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignPresentationStoryBeatConflict);
        }

        presentation.Status = PresentationStatus.Active;
        presentation.ActiveStoryBlockId = selectedStoryBlock.StoryBlockId;
        presentation.CurrentStoryBeatId = currentStoryBeat?.Id;
        presentation.UpdatedAt = updatedAt;
        presentation.EndedAt = null;

        if (shouldAppendEntry)
        {
            var nextSequence = presentation.Id == 0
                ? 1
                : (await _campaignPresentationRepository.GetLatestEntrySequenceAsync(
                    presentation.Id,
                    cancellationToken) ?? 0) + 1;

            presentation.Entries.Add(BuildEntry(
                nextSequence,
                selectedStoryBlock.StoryBlockId,
                currentStoryBeat?.Id,
                updatedAt));
        }

        await _campaignPresentationRepository.SaveChangesAsync(cancellationToken);

        return await BuildPresentationResponseAsync(presentation, cancellationToken);
    }

    public async Task<ServiceResult<CampaignPresentationResponse>> PresentStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1
            || request is null
            || request.StoryBeatId == Guid.Empty)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                validationStatusCode.Value);
        }

        if (!await CampaignSessionExistsAsync(campaignId, sessionId, cancellationToken))
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var presentation = await _campaignPresentationRepository.GetByCampaignSessionIdAsync(
            sessionId,
            cancellationToken);

        if (presentation is null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignPresentationNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            request.StoryBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        var updatedAt = DateTimeOffset.UtcNow;

        if (!await TryApplyStoryBeatSelectionAsync(
            presentation,
            storyBeat,
            updatedAt,
            cancellationToken))
        {
            return new ServiceResult<CampaignPresentationResponse>(
                ApplicationStatusCode.CampaignPresentationStoryBeatConflict);
        }

        presentation.Status = PresentationStatus.Active;
        presentation.ActiveStoryBlockId = storyBeat.StoryBlockId;
        presentation.CurrentStoryBeatId = storyBeat.Id;
        presentation.UpdatedAt = updatedAt;
        presentation.EndedAt = null;

        var latestEntry = await _campaignPresentationRepository.GetLatestEntryAsync(
            presentation.Id,
            cancellationToken);

        if (latestEntry?.StoryBeatId != storyBeat.Id)
        {
            var nextSequence = (await _campaignPresentationRepository.GetLatestEntrySequenceAsync(
                presentation.Id,
                cancellationToken) ?? 0) + 1;

            await _campaignPresentationRepository.AddEntryAsync(
                BuildEntry(
                    nextSequence,
                    storyBeat.StoryBlockId,
                    storyBeat.Id,
                    updatedAt),
                cancellationToken);
        }

        await _campaignPresentationRepository.SaveChangesAsync(cancellationToken);

        return await BuildPresentationResponseAsync(presentation, cancellationToken);
    }

    private async Task<StoryBlock?> ResolveStoryBlockAsync(
        Guid campaignId,
        Guid? requestedStoryBlockId,
        CampaignPresentation? presentation,
        CampaignPresentationEntry? latestEntry,
        CancellationToken cancellationToken)
    {
        var storyBlockId = requestedStoryBlockId
            ?? latestEntry?.StoryBlockId
            ?? presentation?.ActiveStoryBlockId;

        if (storyBlockId is not null)
        {
            return await _storyBlockRepository.GetByCampaignIdAndStoryBlockIdAsync(
                campaignId,
                storyBlockId.Value,
                cancellationToken);
        }

        return await _storyBlockRepository.GetFirstByCampaignIdAsync(
            campaignId,
            cancellationToken);
    }

    private async Task<StoryBeat?> ResolveStoryBeatAsync(
        Guid campaignId,
        Guid storyBlockId,
        CampaignPresentationEntry? latestEntry,
        bool storyBlockWasExplicitlySelected,
        CancellationToken cancellationToken)
    {
        if (!storyBlockWasExplicitlySelected
            && latestEntry?.StoryBeatId is not null)
        {
            var latestStoryBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
                campaignId,
                latestEntry.StoryBeatId.Value,
                cancellationToken);

            if (latestStoryBeat?.StoryBlockId == storyBlockId)
            {
                return latestStoryBeat;
            }
        }

        return await _storyBeatRepository.GetFirstByStoryBlockIdAsync(
            storyBlockId,
            cancellationToken);
    }

    private async Task<bool> CampaignSessionExistsAsync(
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        return session is not null;
    }

    private async Task<bool> TryApplyStoryBeatSelectionAsync(
        CampaignPresentation presentation,
        StoryBeat storyBeat,
        DateTimeOffset selectedAt,
        CancellationToken cancellationToken)
    {
        CampaignPresentationStoryBeatSelection? existingSelection = null;

        if (presentation.Id != 0)
        {
            existingSelection = await _campaignPresentationRepository.GetStoryBeatSelectionAsync(
                presentation.Id,
                storyBeat.StoryBlockId,
                storyBeat.OrderIndex,
                cancellationToken);
        }

        if (existingSelection is not null)
        {
            return existingSelection.SelectedStoryBeatId == storyBeat.Id;
        }

        presentation.StoryBeatSelections.Add(new CampaignPresentationStoryBeatSelection
        {
            StoryBlockId = storyBeat.StoryBlockId,
            OrderIndex = storyBeat.OrderIndex,
            SelectedSecondaryOrderIndex = storyBeat.SecondaryOrderIndex,
            SelectedStoryBeatId = storyBeat.Id,
            SelectedAt = selectedAt
        });

        return true;
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

    private async Task<ServiceResult<CampaignPresentationResponse>> BuildPresentationResponseAsync(
        CampaignPresentation presentation,
        CancellationToken cancellationToken)
    {
        var latestEntry = await _campaignPresentationRepository.GetLatestEntryAsync(
            presentation.Id,
            cancellationToken);
        var selections = await _campaignPresentationRepository.ListStoryBeatSelectionsAsync(
            presentation.Id,
            cancellationToken);

        return new ServiceResult<CampaignPresentationResponse>(
            ApplicationStatusCode.Success,
            ToResponse(presentation, latestEntry, selections));
    }

    private static CampaignPresentationEntry BuildEntry(
        int sequence,
        Guid storyBlockId,
        Guid? storyBeatId,
        DateTimeOffset createdAt)
    {
        return new CampaignPresentationEntry
        {
            Sequence = sequence,
            EntryType = storyBeatId is null
                ? PresentationEntryType.StoryBlockSelected
                : PresentationEntryType.StoryBeatPresented,
            StoryBlockId = storyBlockId,
            StoryBeatId = storyBeatId,
            CreatedAt = createdAt
        };
    }

    private static CampaignPresentationResponse ToResponse(
        CampaignPresentation presentation,
        CampaignPresentationEntry? latestEntry,
        IReadOnlyList<CampaignPresentationStoryBeatSelection> selections)
    {
        return new CampaignPresentationResponse
        {
            Id = presentation.Id,
            CampaignSessionId = presentation.CampaignSessionId,
            Status = presentation.Status,
            ActiveStoryBlockId = presentation.ActiveStoryBlockId,
            CurrentStoryBeatId = presentation.CurrentStoryBeatId,
            StartedAt = presentation.StartedAt,
            UpdatedAt = presentation.UpdatedAt,
            EndedAt = presentation.EndedAt,
            LatestEntry = latestEntry is null
                ? null
                : ToResponse(latestEntry),
            StoryBeatSelections = selections
                .Select(ToResponse)
                .ToList()
        };
    }

    private static CampaignPresentationEntryResponse ToResponse(CampaignPresentationEntry entry)
    {
        return new CampaignPresentationEntryResponse
        {
            Id = entry.Id,
            CampaignPresentationId = entry.CampaignPresentationId,
            Sequence = entry.Sequence,
            EntryType = entry.EntryType,
            StoryBlockId = entry.StoryBlockId,
            StoryBeatId = entry.StoryBeatId,
            CreatedAt = entry.CreatedAt
        };
    }

    private static CampaignPresentationStoryBeatSelectionResponse ToResponse(
        CampaignPresentationStoryBeatSelection selection)
    {
        return new CampaignPresentationStoryBeatSelectionResponse
        {
            Id = selection.Id,
            CampaignPresentationId = selection.CampaignPresentationId,
            StoryBlockId = selection.StoryBlockId,
            OrderIndex = selection.OrderIndex,
            SelectedSecondaryOrderIndex = selection.SelectedSecondaryOrderIndex,
            SelectedStoryBeatId = selection.SelectedStoryBeatId,
            SelectedAt = selection.SelectedAt
        };
    }
}
