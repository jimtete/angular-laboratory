using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Services.CampaignSessionService;
using LearningLab.Services.Helpers;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets.CampaignSessions;

[Authorize(Roles = AccessRoleNames.Master)]
public sealed class CampaignSessionsHub : Hub
{
    private const string SubscribeOperation = "SubscribeToCampaignSessions";
    private const string UpdateOperation = "UpdateCampaignSession";
    private const string UpdateDateOperation = "UpdateCampaignSessionDate";
    private const string UpdateDescriptionOperation = "UpdateCampaignSessionDescription";
    private const string GetSessionNotesOperation = "GetSessionNotes";
    private const string CreateGenericNoteOperation = "CreateGenericSessionNote";
    private const string CreateItemFoundNoteOperation = "CreateItemFoundSessionNote";
    private const string CreateImportantChoiceNoteOperation = "CreateImportantChoiceSessionNote";
    private const string CreateCampaignMilestoneNoteOperation = "CreateCampaignMilestoneSessionNote";
    private const string CreateLevelUpOrMechanicsChangeNoteOperation = "CreateLevelUpOrMechanicsChangeSessionNote";
    private const string AchieveCampaignMilestoneOperation = "AchieveCampaignMilestone";
    private const string UpdateSessionNoteOperation = "UpdateSessionNote";
    private const string UpdateImportantChoiceNoteOperation = "UpdateImportantChoiceSessionNote";
    private const string UpdateCampaignMilestoneNoteOperation = "UpdateCampaignMilestoneSessionNote";
    private const string UpdateLevelUpOrMechanicsChangeNoteOperation = "UpdateLevelUpOrMechanicsChangeSessionNote";
    private const string DeleteSessionNoteOperation = "DeleteSessionNote";

    private readonly ICampaignSessionService _campaignSessionService;
    private readonly ILogger<CampaignSessionsHub> _logger;

    public CampaignSessionsHub(
        ICampaignSessionService campaignSessionService,
        ILogger<CampaignSessionsHub> logger)
    {
        _campaignSessionService = campaignSessionService;
        _logger = logger;
    }

    public async Task SubscribeToCampaignSessions(Guid campaignId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = Context.User is null
            ? null
            : SessionHelper.GetUserId(Context.User);

        if (userId is null)
        {
            await SendSubscribeErrorAsync(
                campaignId,
                "InvalidUserClaim",
                "The access token does not contain a valid user identifier.",
                cancellationToken);
            Context.Abort();
            return;
        }

        ServiceResult<IReadOnlyList<CampaignSessionResponse>> result;

        try
        {
            result = await _campaignSessionService.GetAvailableCampaignSessionsAsync(
                userId.Value,
                campaignId,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Failed to subscribe to campaign sessions. CampaignId: {CampaignId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
                campaignId,
                userId.Value,
                Context.ConnectionId);

            await SendSubscribeErrorAsync(
                campaignId,
                "UnexpectedError",
                "An unexpected error occurred while subscribing to campaign sessions.",
                cancellationToken);

            throw new HubException("An unexpected error occurred while subscribing to campaign sessions.");
        }

        if (result.StatusCode != ApplicationStatusCode.Success)
        {
            var errorMessage = ToHubErrorMessage(result.StatusCode);

            _logger.LogWarning(
                "Campaign sessions subscription rejected. CampaignId: {CampaignId}, UserId: {UserId}, ConnectionId: {ConnectionId}, StatusCode: {StatusCode}",
                campaignId,
                userId.Value,
                Context.ConnectionId,
                result.StatusCode);

            await SendSubscribeErrorAsync(
                campaignId,
                result.StatusCode.ToString(),
                errorMessage,
                cancellationToken);

            throw new HubException(errorMessage);
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SocketGroupNames.CampaignSessions(campaignId),
            cancellationToken);

        await Clients.Caller.SendAsync(
            CampaignSessionSocketClientEvents.CampaignSessionsLoaded,
            result.Data ?? Array.Empty<CampaignSessionResponse>(),
            cancellationToken);
    }

    public Task UnsubscribeFromCampaignSessions(Guid campaignId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SocketGroupNames.CampaignSessions(campaignId),
            Context.ConnectionAborted);
    }

    public async Task UpdateCampaignSession(
        Guid campaignId,
        int sessionId,
        UpdateCampaignSessionRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = Context.User is null
            ? null
            : SessionHelper.GetUserId(Context.User);

        if (userId is null)
        {
            await SendErrorAsync(
                UpdateOperation,
                campaignId,
                "InvalidUserClaim",
                "The access token does not contain a valid user identifier.",
                cancellationToken);
            Context.Abort();
            return;
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateCampaignSessionAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Failed to update campaign session. CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
                campaignId,
                sessionId,
                userId.Value,
                Context.ConnectionId);

            await SendErrorAsync(
                UpdateOperation,
                campaignId,
                "UnexpectedError",
                "An unexpected error occurred while updating the campaign session.",
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the campaign session.");
        }

        if (result.StatusCode == ApplicationStatusCode.Success)
        {
            return;
        }

        var errorMessage = ToHubErrorMessage(result.StatusCode);

        _logger.LogWarning(
            "Campaign session update rejected. CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}, StatusCode: {StatusCode}",
            campaignId,
            sessionId,
            userId.Value,
            Context.ConnectionId,
            result.StatusCode);

        await SendErrorAsync(
            UpdateOperation,
            campaignId,
            result.StatusCode.ToString(),
            errorMessage,
            cancellationToken);

        throw new HubException(errorMessage);
    }

    public async Task<CampaignSessionResponse> UpdateCampaignSessionDate(
        Guid campaignId,
        int sessionId,
        DateTimeOffset? sessionDate)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateDateOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateCampaignSessionDateAsync(
                userId.Value,
                campaignId,
                sessionId,
                sessionDate,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateDateOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the campaign session date.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateDateOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> UpdateCampaignSessionDescription(
        Guid campaignId,
        int sessionId,
        string? description)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateDescriptionOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateCampaignSessionDescriptionAsync(
                userId.Value,
                campaignId,
                sessionId,
                description,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateDescriptionOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the campaign session description.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateDescriptionOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SessionNoteResponse>> GetSessionNotes(
        Guid campaignId,
        int sessionId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            GetSessionNotesOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<IReadOnlyList<SessionNoteResponse>> result;

        try
        {
            result = await _campaignSessionService.GetSessionNotesAsync(
                userId.Value,
                campaignId,
                sessionId,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Failed to get session notes. CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
                campaignId,
                sessionId,
                userId.Value,
                Context.ConnectionId);

            await SendErrorAsync(
                GetSessionNotesOperation,
                campaignId,
                "UnexpectedError",
                "An unexpected error occurred while loading the session notes.",
                cancellationToken);

            throw new HubException("An unexpected error occurred while loading the session notes.");
        }

        if (result.StatusCode == ApplicationStatusCode.Success)
        {
            var notes = result.Data ?? Array.Empty<SessionNoteResponse>();

            await Clients.Caller.SendAsync(
                CampaignSessionSocketClientEvents.SessionNotesLoaded,
                notes,
                cancellationToken);

            return notes;
        }

        var errorMessage = ToHubErrorMessage(result.StatusCode);

        _logger.LogWarning(
            "Session notes load rejected. CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}, StatusCode: {StatusCode}",
            campaignId,
            sessionId,
            userId.Value,
            Context.ConnectionId,
            result.StatusCode);

        await SendErrorAsync(
            GetSessionNotesOperation,
            campaignId,
            result.StatusCode.ToString(),
            errorMessage,
            cancellationToken);

        throw new HubException(errorMessage);
    }

    public async Task<CampaignSessionResponse> CreateGenericSessionNote(
        Guid campaignId,
        int sessionId,
        string? content)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            CreateGenericNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.CreateGenericSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                content,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                CreateGenericNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while creating the session note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            CreateGenericNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> CreateItemFoundSessionNote(
        Guid campaignId,
        int sessionId,
        string? content)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            CreateItemFoundNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.CreateItemFoundSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                content,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                CreateItemFoundNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while creating the item found note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            CreateItemFoundNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> CreateImportantChoiceSessionNote(
        Guid campaignId,
        int sessionId,
        CreateImportantChoiceSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            CreateImportantChoiceNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.CreateImportantChoiceSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                CreateImportantChoiceNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while creating the important choice note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            CreateImportantChoiceNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> CreateCampaignMilestoneSessionNote(
        Guid campaignId,
        int sessionId,
        CreateCampaignMilestoneSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            CreateCampaignMilestoneNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.CreateCampaignMilestoneSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                CreateCampaignMilestoneNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while creating the campaign milestone note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            CreateCampaignMilestoneNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> AchieveCampaignMilestone(
        Guid campaignId,
        int sessionId,
        AchieveCampaignMilestoneRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            AchieveCampaignMilestoneOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.AchieveCampaignMilestoneAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                AchieveCampaignMilestoneOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while achieving the campaign milestone.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            AchieveCampaignMilestoneOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> CreateLevelUpOrMechanicsChangeSessionNote(
        Guid campaignId,
        int sessionId,
        CreateLevelUpOrMechanicsChangeSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            CreateLevelUpOrMechanicsChangeNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.CreateLevelUpOrMechanicsChangeSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                CreateLevelUpOrMechanicsChangeNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while creating the level up or mechanics change note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            CreateLevelUpOrMechanicsChangeNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> UpdateSessionNote(
        Guid campaignId,
        int sessionId,
        int noteId,
        string? content)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateSessionNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                noteId,
                content,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateSessionNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the session note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateSessionNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> UpdateImportantChoiceSessionNote(
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateImportantChoiceSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateImportantChoiceNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateImportantChoiceSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                noteId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateImportantChoiceNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the important choice note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateImportantChoiceNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> UpdateCampaignMilestoneSessionNote(
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateCampaignMilestoneSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateCampaignMilestoneNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateCampaignMilestoneSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                noteId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateCampaignMilestoneNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the campaign milestone note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateCampaignMilestoneNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> UpdateLevelUpOrMechanicsChangeSessionNote(
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateLevelUpOrMechanicsChangeSessionNoteRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            UpdateLevelUpOrMechanicsChangeNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.UpdateLevelUpOrMechanicsChangeSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                noteId,
                request,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                UpdateLevelUpOrMechanicsChangeNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while updating the level up or mechanics change note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            UpdateLevelUpOrMechanicsChangeNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    public async Task<CampaignSessionResponse> DeleteSessionNote(
        Guid campaignId,
        int sessionId,
        int noteId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            DeleteSessionNoteOperation,
            campaignId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        ServiceResult<CampaignSessionResponse> result;

        try
        {
            result = await _campaignSessionService.DeleteSessionNoteAsync(
                userId.Value,
                campaignId,
                sessionId,
                noteId,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await HandleUnexpectedUpdateExceptionAsync(
                DeleteSessionNoteOperation,
                campaignId,
                sessionId,
                userId.Value,
                exception,
                cancellationToken);

            throw new HubException("An unexpected error occurred while deleting the session note.");
        }

        return await HandleCampaignSessionUpdateResultAsync(
            DeleteSessionNoteOperation,
            campaignId,
            sessionId,
            userId.Value,
            result,
            cancellationToken);
    }

    private static string ToHubErrorMessage(ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.UserNotFound => "User was not found.",
            ApplicationStatusCode.CampaignNotFound => "Campaign was not found.",
            ApplicationStatusCode.InvalidCampaignSession => "Campaign session is invalid.",
            ApplicationStatusCode.CampaignSessionNotFound => "Campaign session was not found.",
            ApplicationStatusCode.InvalidSessionNote => "Session note is invalid.",
            ApplicationStatusCode.SessionNoteNotFound => "Session note was not found.",
            ApplicationStatusCode.CampaignParticipantNotFound => "Campaign participant was not found.",
            ApplicationStatusCode.InvalidCampaignMilestone => "Campaign milestone is invalid.",
            ApplicationStatusCode.CampaignMilestoneNotFound => "Campaign milestone was not found.",
            ApplicationStatusCode.CampaignMasterRoleRequired => "Only users with the Master role can view campaign sessions.",
            _ => "An unexpected error occurred."
        };
    }

    private async Task<Guid?> GetUserIdOrAbortAsync(
        string operation,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = Context.User is null
            ? null
            : SessionHelper.GetUserId(Context.User);

        if (userId is not null)
        {
            return userId.Value;
        }

        await SendErrorAsync(
            operation,
            campaignId,
            "InvalidUserClaim",
            "The access token does not contain a valid user identifier.",
            cancellationToken);
        Context.Abort();

        return null;
    }

    private async Task<CampaignSessionResponse> HandleCampaignSessionUpdateResultAsync(
        string operation,
        Guid campaignId,
        int sessionId,
        Guid userId,
        ServiceResult<CampaignSessionResponse> result,
        CancellationToken cancellationToken)
    {
        if (result.StatusCode == ApplicationStatusCode.Success && result.Data is not null)
        {
            return result.Data;
        }

        var errorMessage = ToHubErrorMessage(result.StatusCode);

        _logger.LogWarning(
            "Campaign session update rejected. Operation: {Operation}, CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}, StatusCode: {StatusCode}",
            operation,
            campaignId,
            sessionId,
            userId,
            Context.ConnectionId,
            result.StatusCode);

        await SendErrorAsync(
            operation,
            campaignId,
            result.StatusCode.ToString(),
            errorMessage,
            cancellationToken);

        throw new HubException(errorMessage);
    }

    private async Task HandleUnexpectedUpdateExceptionAsync(
        string operation,
        Guid campaignId,
        int sessionId,
        Guid userId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Failed to update campaign session. Operation: {Operation}, CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
            operation,
            campaignId,
            sessionId,
            userId,
            Context.ConnectionId);

        await SendErrorAsync(
            operation,
            campaignId,
            "UnexpectedError",
            "An unexpected error occurred while updating the campaign session.",
            cancellationToken);
    }

    private Task SendSubscribeErrorAsync(
        Guid? campaignId,
        string errorCode,
        string message,
        CancellationToken cancellationToken)
    {
        return SendErrorAsync(
            SubscribeOperation,
            campaignId,
            errorCode,
            message,
            cancellationToken);
    }

    private Task SendErrorAsync(
        string operation,
        Guid? campaignId,
        string errorCode,
        string message,
        CancellationToken cancellationToken)
    {
        return Clients.Caller.SendAsync(
            CampaignSessionSocketClientEvents.CampaignSessionError,
            new CampaignSessionSocketErrorResponse
            {
                Operation = operation,
                ErrorCode = errorCode,
                Message = message,
                CampaignId = campaignId
            },
            cancellationToken);
    }
}
