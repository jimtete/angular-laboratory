using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Repositories.CampaignParticipationInviteRepository;
using LearningLab.Data.Repositories.CampaignMilestoneRepository;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.CampaignSessionRepository;
using LearningLab.Data.Repositories.SessionNoteRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.CampaignSessions;

namespace LearningLab.Services.CampaignSessionService;

public sealed class CampaignSessionService : ICampaignSessionService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignParticipationInviteRepository _campaignParticipationInviteRepository;
    private readonly ICampaignMilestoneRepository _campaignMilestoneRepository;
    private readonly ICampaignSessionRepository _campaignSessionRepository;
    private readonly IApplicationEventHub _applicationEventHub;
    private readonly ISessionNoteRepository _sessionNoteRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IUserRepository _userRepository;

    public CampaignSessionService(
        ICampaignRepository campaignRepository,
        ICampaignParticipationInviteRepository campaignParticipationInviteRepository,
        ICampaignMilestoneRepository campaignMilestoneRepository,
        ICampaignSessionRepository campaignSessionRepository,
        IApplicationEventHub applicationEventHub,
        ISessionNoteRepository sessionNoteRepository,
        IStoryBeatRepository storyBeatRepository,
        IUserRepository userRepository)
    {
        _campaignRepository = campaignRepository;
        _campaignParticipationInviteRepository = campaignParticipationInviteRepository;
        _campaignMilestoneRepository = campaignMilestoneRepository;
        _campaignSessionRepository = campaignSessionRepository;
        _applicationEventHub = applicationEventHub;
        _sessionNoteRepository = sessionNoteRepository;
        _storyBeatRepository = storyBeatRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignSessionResponse>>> GetAvailableCampaignSessionsAsync(
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
            return new ServiceResult<IReadOnlyList<CampaignSessionResponse>>(
                validationStatusCode.Value);
        }

        var sessions = await _campaignSessionRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);
        var sessionIds = sessions
            .Select(session => session.Id)
            .ToArray();
        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            sessionIds,
            cancellationToken);
        var players = await _campaignParticipationInviteRepository.ListParticipantInformationByCampaignIdAsync(
            campaignId,
            cancellationToken);
        var notesBySessionId = notes.ToLookup(note => note.SessionId);

        return new ServiceResult<IReadOnlyList<CampaignSessionResponse>>(
            ApplicationStatusCode.Success,
            sessions
                .Select(session => ToResponse(session, notesBySessionId[session.Id], players))
                .ToList());
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateCampaignSessionAsync(
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
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var latestSessionNumber = await _campaignSessionRepository.GetLatestSessionNumberByCampaignIdAsync(
            campaignId,
            cancellationToken);
        var nextSessionNumber = (latestSessionNumber ?? 0) + 1;

        var dateCreated = DateTimeOffset.UtcNow;
        var session = new CampaignSession
        {
            CampaignId = campaignId,
            SessionNumber = nextSessionNumber,
            Description = null,
            SessionDate = null,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated
        };

        await _campaignSessionRepository.AddAsync(session, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            [],
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionCreatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        UpdateCampaignSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidCampaignSession);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        session.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        session.SessionDate = request.SessionDate;
        session.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            [session.Id],
            cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            notes,
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionUpdatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionDateAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        DateTimeOffset? sessionDate,
        CancellationToken cancellationToken = default)
    {
        return UpdateCampaignSessionFieldsAsync(
            userId,
            campaignId,
            sessionId,
            session => session.SessionDate = sessionDate,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionDescriptionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? description,
        CancellationToken cancellationToken = default)
    {
        return UpdateCampaignSessionFieldsAsync(
            userId,
            campaignId,
            sessionId,
            session => session.Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            cancellationToken);
    }

    public async Task<ServiceResult<IReadOnlyList<SessionNoteResponse>>> GetSessionNotesAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<IReadOnlyList<SessionNoteResponse>>(
                ApplicationStatusCode.InvalidCampaignSession);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<SessionNoteResponse>>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<IReadOnlyList<SessionNoteResponse>>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var notes = await _sessionNoteRepository.ListBySessionIdAsync(
            session.Id,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<SessionNoteResponse>>(
            ApplicationStatusCode.Success,
            notes.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>> GetSessionPlayersAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>(
                ApplicationStatusCode.InvalidCampaignSession);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var players = await _campaignParticipationInviteRepository.ListParticipantInformationByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>(
            ApplicationStatusCode.Success,
            players);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateGenericSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? content,
        CancellationToken cancellationToken = default)
    {
        var trimmedContent = content?.Trim();

        if (sessionId < 1 || string.IsNullOrWhiteSpace(trimmedContent))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var dateCreated = DateTimeOffset.UtcNow;
        var note = new SessionNote
        {
            SessionId = sessionId,
            Order = nextOrder,
            Type = SessionNoteType.GeneralNotes,
            Content = trimmedContent,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated
        };

        session.UpdatedAt = dateCreated;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            [session.Id],
            cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            notes,
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionUpdatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateItemFoundSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? content,
        CancellationToken cancellationToken = default)
    {
        var trimmedContent = content?.Trim();

        if (sessionId < 1 || string.IsNullOrWhiteSpace(trimmedContent))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var dateCreated = DateTimeOffset.UtcNow;
        var note = new SessionNote
        {
            SessionId = sessionId,
            Order = nextOrder,
            Type = SessionNoteType.ItemFound,
            Content = trimmedContent,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated
        };

        session.UpdatedAt = dateCreated;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            [session.Id],
            cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            notes,
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionUpdatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateImportantChoiceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateImportantChoiceSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || !TryBuildImportantChoiceNote(
                request.Content,
                request.Choices,
                out var trimmedContent,
                out var choices))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        if (sessionId < 1)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var dateCreated = DateTimeOffset.UtcNow;
        var note = new SessionNote
        {
            SessionId = session.Id,
            Order = nextOrder,
            Type = SessionNoteType.ImportantChoice,
            Content = trimmedContent,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated,
            Choices = choices.ToList()
        };

        session.UpdatedAt = dateCreated;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateCampaignMilestoneSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateCampaignMilestoneSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || sessionId < 1)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var dateCreated = DateTimeOffset.UtcNow;
        CampaignMilestone? milestoneToAdd = null;
        var trimmedContent = request.Content?.Trim();

        if (request.MilestoneId is not null)
        {
            if (request.MilestoneId < 1)
            {
                return new ServiceResult<CampaignSessionResponse>(
                    ApplicationStatusCode.InvalidSessionNote);
            }

            var existingMilestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
                campaignId,
                request.MilestoneId.Value,
                cancellationToken);

            if (existingMilestone is null)
            {
                return new ServiceResult<CampaignSessionResponse>(
                    ApplicationStatusCode.CampaignMilestoneNotFound);
            }

            if (existingMilestone.AchievedAt is not null)
            {
                return new ServiceResult<CampaignSessionResponse>(
                    ApplicationStatusCode.InvalidCampaignMilestone);
            }

            trimmedContent = string.IsNullOrWhiteSpace(trimmedContent)
                ? existingMilestone.Title
                : trimmedContent;
            existingMilestone.AchievedAt = dateCreated;
            existingMilestone.UpdatedAt = dateCreated;
        }
        else
        {
            if (!TryBuildCampaignMilestoneNote(
                    request.Content,
                    request.Milestone,
                    out var builtContent,
                    out var milestone))
            {
                return new ServiceResult<CampaignSessionResponse>(
                    ApplicationStatusCode.InvalidSessionNote);
            }

            trimmedContent = builtContent;
            milestone.CampaignId = campaignId;
            milestone.CreatedAt = dateCreated;
            milestone.UpdatedAt = dateCreated;
            milestoneToAdd = milestone;
        }

        if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var note = new SessionNote
        {
            SessionId = session.Id,
            Order = nextOrder,
            Type = SessionNoteType.CampaignMilestone,
            Content = trimmedContent,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated
        };

        session.UpdatedAt = dateCreated;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        if (milestoneToAdd is not null)
        {
            await _campaignMilestoneRepository.AddAsync(milestoneToAdd, cancellationToken);
        }
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignSessionResponse>> AchieveCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        AchieveCampaignMilestoneRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Task.FromResult(new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidCampaignMilestone));
        }

        return CreateCampaignMilestoneSessionNoteAsync(
            userId,
            campaignId,
            sessionId,
            new CreateCampaignMilestoneSessionNoteRequest
            {
                MilestoneId = request.MilestoneId,
                Content = request.Content
            },
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateLevelUpOrMechanicsChangeSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateLevelUpOrMechanicsChangeSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || sessionId < 1
            || !TryBuildLevelUpOrMechanicsChangeNote(
                request.Content,
                request.MechanicsChanges,
                out var trimmedContent,
                out var mechanicsChanges,
                out var playerIds))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        if (!await AllPlayersParticipateInCampaignAsync(
                campaignId,
                playerIds,
                cancellationToken))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignParticipantNotFound);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var dateCreated = DateTimeOffset.UtcNow;
        var note = new SessionNote
        {
            SessionId = session.Id,
            Order = nextOrder,
            Type = SessionNoteType.LevelUpOrMechanicsChange,
            Content = trimmedContent,
            CreatedAt = dateCreated,
            UpdatedAt = dateCreated,
            MechanicsChanges = mechanicsChanges.ToList()
        };

        session.UpdatedAt = dateCreated;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateStoryBeatPlayedSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        string? content = null,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1 || storyBeatId == Guid.Empty)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        var existingNote = await _sessionNoteRepository.GetBySessionIdAndFullStoryBeatAsync(
            session.Id,
            storyBeat.Id,
            cancellationToken);

        if (existingNote is not null)
        {
            return await BuildAndPublishUpdatedSessionResponseAsync(
                session,
                cancellationToken);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var timestamp = DateTimeOffset.UtcNow;
        var noteContent = string.IsNullOrWhiteSpace(content)
            ? BuildStoryBeatPlayedNoteContent(storyBeat.Title)
            : content.Trim();

        var note = new SessionNote
        {
            SessionId = session.Id,
            Order = nextOrder,
            Type = SessionNoteType.StoryBeatPlayed,
            Content = noteContent,
            StoryBeatId = storyBeat.Id,
            StoryBeat = storyBeat,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        session.UpdatedAt = timestamp;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> CreateStoryBeatReferenceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        Guid? referenceId = null,
        string? content = null,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1
            || storyBeatId == Guid.Empty
            || !Enum.IsDefined(referenceType))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        if (!TryResolveStoryBeatReference(
                storyBeat,
                referenceType,
                referenceId,
                out var normalizedReferenceId,
                out var npcTag,
                out var contentSnapshot))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var timestamp = DateTimeOffset.UtcNow;

        var existingNote = await _sessionNoteRepository.GetBySessionIdAndStoryBeatReferenceAsync(
            session.Id,
            storyBeat.Id,
            referenceType,
            normalizedReferenceId,
            cancellationToken);

        if (existingNote is not null)
        {
            return await BuildAndPublishUpdatedSessionResponseAsync(
                session,
                cancellationToken);
        }

        var aggregateNote = await _sessionNoteRepository.GetBySessionIdAndStoryBeatReferenceTypeAsync(
            session.Id,
            storyBeat.Id,
            referenceType,
            cancellationToken);

        if (aggregateNote is not null)
        {
            aggregateNote.StoryBeatReferences.Add(new SessionNoteStoryBeatReference
            {
                StoryBeatId = storyBeat.Id,
                StoryBeat = storyBeat,
                ReferenceType = referenceType,
                ReferenceId = normalizedReferenceId,
                ReferenceOutcome = SessionNoteStoryBeatReferenceOutcome.Presented,
                NpcTag = npcTag,
                ContentSnapshot = contentSnapshot,
                CreatedAt = timestamp
            });
            aggregateNote.Content = string.IsNullOrWhiteSpace(content)
                ? BuildStoryBeatReferenceNoteContent(storyBeat.Title, referenceType)
                : content.Trim();
            aggregateNote.UpdatedAt = timestamp;
            session.UpdatedAt = timestamp;

            await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

            return await BuildAndPublishUpdatedSessionResponseAsync(
                session,
                cancellationToken);
        }

        var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
            session.Id,
            cancellationToken);
        var nextOrder = (latestOrder ?? 0) + 1;
        var noteContent = string.IsNullOrWhiteSpace(content)
            ? BuildStoryBeatReferenceNoteContent(storyBeat.Title, referenceType)
            : content.Trim();

        var note = new SessionNote
        {
            SessionId = session.Id,
            Order = nextOrder,
            Type = SessionNoteType.StoryBeatPlayed,
            Content = noteContent,
            StoryBeatId = storyBeat.Id,
            StoryBeat = storyBeat,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            StoryBeatReferences =
            [
                new SessionNoteStoryBeatReference
                {
                    StoryBeatId = storyBeat.Id,
                    StoryBeat = storyBeat,
                    ReferenceType = referenceType,
                    ReferenceId = normalizedReferenceId,
                    ReferenceOutcome = SessionNoteStoryBeatReferenceOutcome.Presented,
                    NpcTag = npcTag,
                    ContentSnapshot = contentSnapshot,
                    CreatedAt = timestamp
                }
            ]
        };

        session.UpdatedAt = timestamp;
        await _sessionNoteRepository.AddAsync(note, cancellationToken);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> TakeDecisionStoryBeatOptionSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        Guid decisionOptionId,
        string? content = null,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1
            || storyBeatId == Guid.Empty
            || decisionOptionId == Guid.Empty)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        if (storyBeat.StoryBeatType != StoryBeatType.Decision
            || storyBeat.Decision is null
            || storyBeat.Decision.Decisions.Count == 0)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var decisions = storyBeat.Decision.Decisions
            .OrderBy(decision => decision.OrderIndex)
            .ToList();
        var selectedDecision = decisions
            .SingleOrDefault(decision => ResolveDecisionOptionId(storyBeat.Id, decision) == decisionOptionId);

        if (selectedDecision is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var note = await _sessionNoteRepository.GetBySessionIdAndStoryBeatReferenceTypeAsync(
            session.Id,
            storyBeat.Id,
            SessionNoteStoryBeatReferenceType.DecisionOption,
            cancellationToken);

        if (note is null)
        {
            var latestOrder = await _sessionNoteRepository.GetLatestOrderBySessionIdAsync(
                session.Id,
                cancellationToken);

            note = new SessionNote
            {
                SessionId = session.Id,
                Order = (latestOrder ?? 0) + 1,
                Type = SessionNoteType.StoryBeatPlayed,
                StoryBeatId = storyBeat.Id,
                StoryBeat = storyBeat,
                CreatedAt = timestamp
            };

            await _sessionNoteRepository.AddAsync(note, cancellationToken);
        }
        else
        {
            foreach (var reference in note.StoryBeatReferences
                .Where(reference => reference.ReferenceType == SessionNoteStoryBeatReferenceType.DecisionOption)
                .ToList())
            {
                note.StoryBeatReferences.Remove(reference);
                _sessionNoteRepository.RemoveStoryBeatReference(reference);
            }
        }

        note.Content = string.IsNullOrWhiteSpace(content)
            ? BuildDecisionStoryBeatNoteContent(storyBeat.Title, selectedDecision)
            : content.Trim();
        note.UpdatedAt = timestamp;

        foreach (var decision in decisions)
        {
            var optionId = ResolveDecisionOptionId(storyBeat.Id, decision);

            note.StoryBeatReferences.Add(new SessionNoteStoryBeatReference
            {
                StoryBeatId = storyBeat.Id,
                StoryBeat = storyBeat,
                ReferenceType = SessionNoteStoryBeatReferenceType.DecisionOption,
                ReferenceId = optionId,
                ReferenceOutcome = optionId == decisionOptionId
                    ? SessionNoteStoryBeatReferenceOutcome.Taken
                    : SessionNoteStoryBeatReferenceOutcome.Presented,
                ContentSnapshot = BuildDecisionOptionSnapshot(decision),
                CreatedAt = timestamp
            });
        }

        session.UpdatedAt = timestamp;
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateStoryBeatReferenceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        UpdateStoryBeatReferenceSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1
            || request.StoryBeatId == Guid.Empty
            || !Enum.IsDefined(request.ReferenceType))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var existingNote = await _sessionNoteRepository.GetBySessionIdAndStoryBeatReferenceAsync(
            session.Id,
            request.StoryBeatId,
            request.ReferenceType,
            request.ReferenceId,
            cancellationToken);

        if (request.IsPlayed)
        {
            if (existingNote is not null)
            {
                return await BuildAndPublishUpdatedSessionResponseAsync(
                    session,
                    cancellationToken);
            }

            return await CreateStoryBeatReferenceSessionNoteAsync(
                userId,
                campaignId,
                sessionId,
                request.StoryBeatId,
                request.ReferenceType,
                request.ReferenceId,
                request.Content,
                cancellationToken);
        }

        if (existingNote is null)
        {
            return await BuildAndPublishUpdatedSessionResponseAsync(
                session,
                cancellationToken);
        }

        var existingReference = existingNote.StoryBeatReferences
            .SingleOrDefault(reference =>
                reference.StoryBeatId == request.StoryBeatId
                && reference.ReferenceType == request.ReferenceType
                && reference.ReferenceId == request.ReferenceId);

        if (existingReference is null)
        {
            return await BuildAndPublishUpdatedSessionResponseAsync(
                session,
                cancellationToken);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var remainingReferenceCount = existingNote.StoryBeatReferences.Count - 1;
        session.UpdatedAt = timestamp;

        if (remainingReferenceCount <= 0)
        {
            var deletedOrder = existingNote.Order;

            _sessionNoteRepository.Remove(existingNote);
            await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

            await _sessionNoteRepository.DecrementOrderAfterAsync(
                session.Id,
                deletedOrder,
                cancellationToken);
        }
        else
        {
            existingNote.StoryBeatReferences.Remove(existingReference);
            _sessionNoteRepository.RemoveStoryBeatReference(existingReference);
            existingNote.UpdatedAt = timestamp;
            await _campaignSessionRepository.SaveChangesAsync(cancellationToken);
        }

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        string? content,
        CancellationToken cancellationToken = default)
    {
        var trimmedContent = content?.Trim();

        if (sessionId < 1 || noteId < 1 || string.IsNullOrWhiteSpace(trimmedContent))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var note = await _sessionNoteRepository.GetBySessionIdAndNoteIdAsync(
            session.Id,
            noteId,
            cancellationToken);

        if (note is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.SessionNoteNotFound);
        }

        var updatedAt = DateTimeOffset.UtcNow;
        note.Content = trimmedContent;
        note.UpdatedAt = updatedAt;
        session.UpdatedAt = updatedAt;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateImportantChoiceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateImportantChoiceSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || sessionId < 1
            || noteId < 1
            || !TryBuildImportantChoiceNote(
                request.Content,
                request.Choices,
                out var trimmedContent,
                out var choices))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var note = await _sessionNoteRepository.GetBySessionIdAndNoteIdAsync(
            session.Id,
            noteId,
            cancellationToken);

        if (note is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.SessionNoteNotFound);
        }

        if (note.Type != SessionNoteType.ImportantChoice)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var updatedAt = DateTimeOffset.UtcNow;
        note.Content = trimmedContent;
        note.UpdatedAt = updatedAt;
        note.Choices.Clear();

        foreach (var choice in choices)
        {
            note.Choices.Add(choice);
        }

        session.UpdatedAt = updatedAt;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignMilestoneSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateCampaignMilestoneSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || sessionId < 1
            || noteId < 1
            || request.MilestoneId < 1
            || !TryBuildCampaignMilestoneNote(
                request.Content,
                request.Milestone,
                out var trimmedContent,
                out var milestone))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var note = await _sessionNoteRepository.GetBySessionIdAndNoteIdAsync(
            session.Id,
            noteId,
            cancellationToken);

        if (note is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.SessionNoteNotFound);
        }

        if (note.Type != SessionNoteType.CampaignMilestone)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var existingMilestone = await _campaignMilestoneRepository.GetByCampaignIdAndMilestoneIdAsync(
            campaignId,
            request.MilestoneId,
            cancellationToken);

        if (existingMilestone is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var updatedAt = DateTimeOffset.UtcNow;
        note.Content = trimmedContent;
        note.UpdatedAt = updatedAt;
        existingMilestone.Title = milestone.Title;
        existingMilestone.Description = milestone.Description;
        existingMilestone.AchievedAt = milestone.AchievedAt;
        existingMilestone.Importance = milestone.Importance;
        existingMilestone.UpdatedAt = updatedAt;

        session.UpdatedAt = updatedAt;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> UpdateLevelUpOrMechanicsChangeSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateLevelUpOrMechanicsChangeSessionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || sessionId < 1
            || noteId < 1
            || !TryBuildLevelUpOrMechanicsChangeNote(
                request.Content,
                request.MechanicsChanges,
                out var trimmedContent,
                out var mechanicsChanges,
                out var playerIds))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        if (!await AllPlayersParticipateInCampaignAsync(
                campaignId,
                playerIds,
                cancellationToken))
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignParticipantNotFound);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var note = await _sessionNoteRepository.GetBySessionIdAndNoteIdAsync(
            session.Id,
            noteId,
            cancellationToken);

        if (note is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.SessionNoteNotFound);
        }

        if (note.Type != SessionNoteType.LevelUpOrMechanicsChange)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var updatedAt = DateTimeOffset.UtcNow;
        note.Content = trimmedContent;
        note.UpdatedAt = updatedAt;
        note.MechanicsChanges.Clear();

        foreach (var mechanicsChange in mechanicsChanges)
        {
            note.MechanicsChanges.Add(mechanicsChange);
        }

        session.UpdatedAt = updatedAt;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    public async Task<ServiceResult<CampaignSessionResponse>> DeleteSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        CancellationToken cancellationToken = default)
    {
        if (sessionId < 1 || noteId < 1)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidSessionNote);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        var note = await _sessionNoteRepository.GetBySessionIdAndNoteIdAsync(
            session.Id,
            noteId,
            cancellationToken);

        if (note is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.SessionNoteNotFound);
        }

        var deletedOrder = note.Order;
        session.UpdatedAt = DateTimeOffset.UtcNow;
        _sessionNoteRepository.Remove(note);
        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        await _sessionNoteRepository.DecrementOrderAfterAsync(
            session.Id,
            deletedOrder,
            cancellationToken);

        return await BuildAndPublishUpdatedSessionResponseAsync(
            session,
            cancellationToken);
    }

    private async Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionFieldsAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Action<CampaignSession> applyUpdate,
        CancellationToken cancellationToken)
    {
        if (sessionId < 1)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.InvalidCampaignSession);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                validationStatusCode.Value);
        }

        var session = await _campaignSessionRepository.GetByCampaignIdAndSessionIdAsync(
            campaignId,
            sessionId,
            cancellationToken);

        if (session is null)
        {
            return new ServiceResult<CampaignSessionResponse>(
                ApplicationStatusCode.CampaignSessionNotFound);
        }

        applyUpdate(session);
        session.UpdatedAt = DateTimeOffset.UtcNow;

        await _campaignSessionRepository.SaveChangesAsync(cancellationToken);

        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            [session.Id],
            cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            notes,
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionUpdatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
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

    private async Task<ServiceResult<CampaignSessionResponse>> BuildAndPublishUpdatedSessionResponseAsync(
        CampaignSession session,
        CancellationToken cancellationToken)
    {
        var notes = await _sessionNoteRepository.ListBySessionIdsAsync(
            [session.Id],
            cancellationToken);
        var response = await BuildSessionResponseAsync(
            session,
            notes,
            cancellationToken);

        await _applicationEventHub.PublishAsync(
            new CampaignSessionUpdatedEvent(response),
            cancellationToken);

        return new ServiceResult<CampaignSessionResponse>(
            ApplicationStatusCode.Success,
            response);
    }

    private async Task<CampaignSessionResponse> BuildSessionResponseAsync(
        CampaignSession session,
        IEnumerable<SessionNote> notes,
        CancellationToken cancellationToken)
    {
        var players = await _campaignParticipationInviteRepository.ListParticipantInformationByCampaignIdAsync(
            session.CampaignId,
            cancellationToken);

        return ToResponse(
            session,
            notes,
            players);
    }

    private async Task<bool> AllPlayersParticipateInCampaignAsync(
        Guid campaignId,
        IReadOnlyCollection<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        var participantCount = await _campaignParticipationInviteRepository.CountParticipationsByUserIdsAsync(
            campaignId,
            playerIds,
            cancellationToken);

        return participantCount == playerIds.Count;
    }

    private static bool TryBuildImportantChoiceNote(
        string? content,
        IReadOnlyList<SessionNoteChoiceRequest>? choices,
        out string trimmedContent,
        out IReadOnlyList<SessionNoteChoice> sessionNoteChoices)
    {
        trimmedContent = string.Empty;
        sessionNoteChoices = [];

        if (string.IsNullOrWhiteSpace(content)
            || choices is null
            || choices.Count < 2
            || choices.Any(choice => choice is null))
        {
            return false;
        }

        var normalizedChoices = choices
            .Select((choice, index) => new
            {
                Order = index + 1,
                ChoiceText = choice.ChoiceText?.Trim(),
                choice.IsChosen
            })
            .ToArray();

        if (normalizedChoices.Any(choice => string.IsNullOrWhiteSpace(choice.ChoiceText)))
        {
            return false;
        }

        trimmedContent = content.Trim();
        sessionNoteChoices = normalizedChoices
            .Select(choice => new SessionNoteChoice
            {
                Order = choice.Order,
                ChoiceText = choice.ChoiceText!,
                IsChosen = choice.IsChosen
            })
            .ToList();

        return true;
    }

    private static bool TryBuildLevelUpOrMechanicsChangeNote(
        string? content,
        IReadOnlyList<SessionNoteMechanicsChangeRequest>? changes,
        out string trimmedContent,
        out IReadOnlyList<SessionNoteMechanicsChange> mechanicsChanges,
        out IReadOnlyList<Guid> playerIds)
    {
        trimmedContent = string.Empty;
        mechanicsChanges = [];
        playerIds = [];

        if (string.IsNullOrWhiteSpace(content)
            || changes is null
            || changes.Count == 0
            || changes.Any(change => change is null))
        {
            return false;
        }

        var normalizedChanges = changes
            .Select((change, index) => new
            {
                Order = index + 1,
                change.PlayerId,
                ChangeText = string.IsNullOrWhiteSpace(change.ChangeText)
                    ? null
                    : change.ChangeText.Trim()
            })
            .ToArray();

        if (normalizedChanges.Any(change => change.PlayerId == Guid.Empty))
        {
            return false;
        }

        playerIds = normalizedChanges
            .Select(change => change.PlayerId)
            .Distinct()
            .ToArray();

        if (playerIds.Count != normalizedChanges.Length)
        {
            return false;
        }

        trimmedContent = content.Trim();
        mechanicsChanges = normalizedChanges
            .Select(change => new SessionNoteMechanicsChange
            {
                Order = change.Order,
                PlayerId = change.PlayerId,
                ChangeText = change.ChangeText
            })
            .ToList();

        return true;
    }

    private static bool TryBuildCampaignMilestoneNote(
        string? content,
        CampaignMilestoneRequest? request,
        out string trimmedContent,
        out CampaignMilestone milestone)
    {
        trimmedContent = string.Empty;
        milestone = new CampaignMilestone();

        var title = request?.Title?.Trim();
        var description = string.IsNullOrWhiteSpace(request?.Description)
            ? null
            : request.Description.Trim();

        if (string.IsNullOrWhiteSpace(content)
            || string.IsNullOrWhiteSpace(title)
            || request is null
            || !Enum.IsDefined(request.Importance))
        {
            return false;
        }

        trimmedContent = content.Trim();
        milestone = new CampaignMilestone
        {
            Title = title,
            Description = description,
            AchievedAt = request.AchievedAt,
            Importance = request.Importance
        };

        return true;
    }

    private static CampaignSessionResponse ToResponse(
        CampaignSession session,
        IEnumerable<SessionNote> notes,
        IReadOnlyList<CampaignMemberInformationResponse> players)
    {
        return new CampaignSessionResponse
        {
            Id = session.Id,
            CampaignId = session.CampaignId,
            SessionNumber = session.SessionNumber,
            Description = session.Description,
            SessionDate = session.SessionDate,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Players = players,
            Notes = notes.Select(ToResponse).ToList()
        };
    }

    private static SessionNoteResponse ToResponse(SessionNote note)
    {
        return new SessionNoteResponse
        {
            Id = note.Id,
            SessionId = note.SessionId,
            Order = note.Order,
            Type = note.Type,
            Content = note.Content,
            StoryBeatId = note.StoryBeatId,
            StoryBeat = note.StoryBeat is null
                ? null
                : ToStoryBeatReferenceResponse(note.StoryBeat),
            StoryBeatReferences = note.StoryBeatReferences
                .OrderBy(reference => reference.CreatedAt)
                .ThenBy(reference => reference.Id)
                .Select(ToResponse)
                .ToList(),
            Choices = note.Choices
                .OrderBy(choice => choice.Order)
                .ThenBy(choice => choice.Id)
                .Select(ToResponse)
                .ToList(),
            MechanicsChanges = note.MechanicsChanges
                .OrderBy(change => change.Order)
                .ThenBy(change => change.Id)
                .Select(ToResponse)
                .ToList(),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    private static SessionNoteStoryBeatReferenceResponse ToResponse(
        SessionNoteStoryBeatReference reference)
    {
        return new SessionNoteStoryBeatReferenceResponse
        {
            Id = reference.Id,
            SessionNoteId = reference.SessionNoteId,
            StoryBeatId = reference.StoryBeatId,
            ReferenceType = reference.ReferenceType,
            ReferenceId = reference.ReferenceId,
            ReferenceOutcome = reference.ReferenceOutcome,
            NpcTag = reference.NpcTag,
            ContentSnapshot = reference.ContentSnapshot,
            CreatedAt = reference.CreatedAt
        };
    }

    private static SessionNoteStoryBeatResponse ToStoryBeatReferenceResponse(StoryBeat storyBeat)
    {
        return new SessionNoteStoryBeatResponse
        {
            StoryBeatId = storyBeat.Id,
            StoryBlockId = storyBeat.StoryBlockId,
            OrderIndex = storyBeat.OrderIndex,
            SecondaryOrderIndex = storyBeat.SecondaryOrderIndex,
            Title = storyBeat.Title,
            StoryBeatType = storyBeat.StoryBeatType
        };
    }

    private static string BuildStoryBeatPlayedNoteContent(string storyBeatTitle)
    {
        return $"Story beat played: {storyBeatTitle}";
    }

    private static string BuildStoryBeatReferenceNoteContent(
        string storyBeatTitle,
        SessionNoteStoryBeatReferenceType referenceType)
    {
        return referenceType switch
        {
            SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction =>
                $"Roleplaying NPC interactions played in {storyBeatTitle}",
            SessionNoteStoryBeatReferenceType.RoleplayingInformation =>
                $"Roleplaying information given in {storyBeatTitle}",
            SessionNoteStoryBeatReferenceType.DecisionOption =>
                $"Decision options presented in {storyBeatTitle}",
            _ => BuildStoryBeatPlayedNoteContent(storyBeatTitle)
        };
    }

    private static bool TryResolveStoryBeatReference(
        StoryBeat storyBeat,
        SessionNoteStoryBeatReferenceType referenceType,
        Guid? referenceId,
        out Guid? normalizedReferenceId,
        out string? npcTag,
        out string contentSnapshot)
    {
        normalizedReferenceId = referenceId;
        npcTag = null;
        contentSnapshot = storyBeat.Title;

        switch (referenceType)
        {
            case SessionNoteStoryBeatReferenceType.StoryBeat:
                normalizedReferenceId = null;
                return true;

            case SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction:
            {
                if (referenceId is null || storyBeat.Roleplaying is null)
                {
                    return false;
                }

                var npcReference = storyBeat.Roleplaying.NpcReferences
                    .SingleOrDefault(reference => reference.Id == referenceId.Value);

                if (npcReference is null)
                {
                    return false;
                }

                npcTag = npcReference.NpcTag;
                contentSnapshot = npcReference.NpcTag;
                return true;
            }

            case SessionNoteStoryBeatReferenceType.RoleplayingInformation:
            {
                if (referenceId is null || storyBeat.Roleplaying is null)
                {
                    return false;
                }

                var information = storyBeat.Roleplaying.DiscoverableInformation
                    .SingleOrDefault(item => item.Id == referenceId.Value);

                if (information is null)
                {
                    return false;
                }

                npcTag = string.IsNullOrWhiteSpace(information.NpcTag)
                    ? null
                    : information.NpcTag;
                contentSnapshot = information.Information;
                return true;
            }

            case SessionNoteStoryBeatReferenceType.DecisionOption:
            {
                if (referenceId is null || storyBeat.Decision is null)
                {
                    return false;
                }

                var decision = storyBeat.Decision.Decisions
                    .SingleOrDefault(item => ResolveDecisionOptionId(storyBeat.Id, item) == referenceId.Value);

                if (decision is null)
                {
                    return false;
                }

                contentSnapshot = BuildDecisionOptionSnapshot(decision);
                return true;
            }

            default:
                return false;
        }
    }

    private static string BuildDecisionStoryBeatNoteContent(
        string storyBeatTitle,
        StoryBeatDecisionOption selectedDecision)
    {
        return $"Decision taken in {storyBeatTitle}: {selectedDecision.Title}";
    }

    private static string BuildDecisionOptionSnapshot(StoryBeatDecisionOption decision)
    {
        return $"{decision.Title}: {decision.Description}";
    }

    private static Guid ResolveDecisionOptionId(
        Guid storyBeatId,
        StoryBeatDecisionOption decision)
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

    private static SessionNoteChoiceResponse ToResponse(SessionNoteChoice choice)
    {
        return new SessionNoteChoiceResponse
        {
            Id = choice.Id,
            SessionNoteId = choice.SessionNoteId,
            Order = choice.Order,
            ChoiceText = choice.ChoiceText,
            IsChosen = choice.IsChosen
        };
    }

    private static SessionNoteMechanicsChangeResponse ToResponse(SessionNoteMechanicsChange change)
    {
        return new SessionNoteMechanicsChangeResponse
        {
            Id = change.Id,
            SessionNoteId = change.SessionNoteId,
            Order = change.Order,
            PlayerId = change.PlayerId,
            ChangeText = change.ChangeText
        };
    }

}
