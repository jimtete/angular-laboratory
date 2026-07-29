using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Hub;
using LearningLab.Presentation.Models;
using LearningLab.Services.Helpers;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets.PresentationMode;

[Authorize(Roles = AccessRoleNames.Master)]
public sealed class PresentationModeSocketHub : Hub
{
    private const string SubscribeOperation = "SubscribeToPresentationMode";
    private const string GetOperation = "GetPresentationMode";
    private const string GetStoryBlockOperation = "GetPresentationModeStoryBlock";
    private const string EnableOperation = "EnablePresentationMode";
    private const string DisableOperation = "DisablePresentationMode";
    private const string PresentStoryBeatOperation = "PresentStoryBeat";
    private const string FinishStoryBeatOperation = "FinishStoryBeat";
    private const string MarkStoryBeatReferenceOperation = "MarkStoryBeatReference";
    private const string MarkRoleplayingInformationGivenOperation = "MarkRoleplayingInformationGiven";
    private const string MarkRoleplayingNpcInteractionGivenOperation = "MarkRoleplayingNpcInteractionGiven";
    private const string TakeDecisionOptionOperation = "TakeDecisionOption";

    private readonly IPresentationModeHub _presentationModeHub;
    private readonly ILogger<PresentationModeSocketHub> _logger;

    public PresentationModeSocketHub(
        IPresentationModeHub presentationModeHub,
        ILogger<PresentationModeSocketHub> logger)
    {
        _presentationModeHub = presentationModeHub;
        _logger = logger;
    }

    public async Task SubscribeToPresentationMode(
        Guid campaignId,
        int sessionId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            SubscribeOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId),
            cancellationToken);

        var result = await ExecutePresentationOperationAsync(
            SubscribeOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.GetPresentationModeWorkspaceAsync(
                userId.Value,
                campaignId,
                sessionId,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return;
        }

        await Clients.Caller.SendAsync(
            PresentationModeSocketClientEvents.PresentationModeLoaded,
            result,
            cancellationToken);
    }

    public Task UnsubscribeFromPresentationMode(
        Guid campaignId,
        int sessionId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId),
            Context.ConnectionAborted);
    }

    public async Task<PresentationModeWorkspaceResponse?> GetPresentationMode(
        Guid campaignId,
        int sessionId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            GetOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            GetOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.GetPresentationModeWorkspaceAsync(
                userId.Value,
                campaignId,
                sessionId,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients.Caller.SendAsync(
            PresentationModeSocketClientEvents.PresentationModeLoaded,
            result,
            cancellationToken);

        return result;
    }

    public async Task<PresentationModeWorkspaceResponse?> EnablePresentationMode(
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            EnableOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            EnableOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.EnablePresentationModeAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients
            .Group(SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId))
            .SendAsync(
                PresentationModeSocketClientEvents.PresentationModeEnabled,
                result,
                cancellationToken);

        return result;
    }

    public async Task<PresentationModeStoryBlockResponse?> GetPresentationModeStoryBlock(
        Guid campaignId,
        int sessionId,
        Guid storyBlockId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            GetStoryBlockOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            GetStoryBlockOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.GetPresentationModeStoryBlockAsync(
                userId.Value,
                campaignId,
                sessionId,
                storyBlockId,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients.Caller.SendAsync(
            PresentationModeSocketClientEvents.PresentationModeStoryBlockLoaded,
            result,
            cancellationToken);

        return result;
    }

    public async Task<PresentationModeWorkspaceResponse?> DisablePresentationMode(
        Guid campaignId,
        int sessionId)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            DisableOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync<PresentationModeWorkspaceResponse>(
            DisableOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.DisablePresentationModeWorkspaceAsync(
                userId.Value,
                campaignId,
                sessionId,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients
            .Group(SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId))
            .SendAsync(
                PresentationModeSocketClientEvents.PresentationModeDisabled,
                result,
                cancellationToken);

        return result;
    }

    public async Task<PresentationModeWorkspaceResponse?> PresentStoryBeat(
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            PresentStoryBeatOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            PresentStoryBeatOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.PresentStoryBeatWorkspaceAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients
            .Group(SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId))
            .SendAsync(
                PresentationModeSocketClientEvents.PresentationModeUpdated,
                result,
                cancellationToken);

        return result;
    }

    public async Task<PresentationModeStoryBeatPlayedResponse?> FinishStoryBeat(
        Guid campaignId,
        int sessionId,
        FinishPresentationStoryBeatRequest request)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            FinishStoryBeatOperation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            FinishStoryBeatOperation,
            campaignId,
            sessionId,
            userId.Value,
            () => _presentationModeHub.FinishStoryBeatAsync(
                userId.Value,
                campaignId,
                sessionId,
                request,
                cancellationToken),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        _ = Clients
            .OthersInGroup(SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId))
            .SendAsync(
                PresentationModeSocketClientEvents.PresentationModeStoryBeatPlayed,
                result,
                CancellationToken.None);

        return result;
    }

    public Task<PresentationModeStoryBeatReferenceMarkedResponse?> MarkStoryBeatReference(
        Guid campaignId,
        int sessionId,
        MarkPresentationStoryBeatReferenceRequest request)
    {
        return MarkStoryBeatReferenceAsync(
            MarkStoryBeatReferenceOperation,
            campaignId,
            sessionId,
            userId => _presentationModeHub.MarkStoryBeatReferenceAsync(
                userId,
                campaignId,
                sessionId,
                request,
                Context.ConnectionAborted));
    }

    public Task<PresentationModeStoryBeatReferenceMarkedResponse?> MarkRoleplayingInformationGiven(
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingInformationRequest request)
    {
        return MarkStoryBeatReferenceAsync(
            MarkRoleplayingInformationGivenOperation,
            campaignId,
            sessionId,
            userId => _presentationModeHub.MarkRoleplayingInformationGivenAsync(
                userId,
                campaignId,
                sessionId,
                request,
                Context.ConnectionAborted));
    }

    public Task<PresentationModeStoryBeatReferenceMarkedResponse?> MarkRoleplayingNpcInteractionGiven(
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingNpcInteractionRequest request)
    {
        return MarkStoryBeatReferenceAsync(
            MarkRoleplayingNpcInteractionGivenOperation,
            campaignId,
            sessionId,
            userId => _presentationModeHub.MarkRoleplayingNpcInteractionGivenAsync(
                userId,
                campaignId,
                sessionId,
                request,
                Context.ConnectionAborted));
    }

    public Task<PresentationModeStoryBeatReferenceMarkedResponse?> TakeDecisionOption(
        Guid campaignId,
        int sessionId,
        TakePresentationDecisionOptionRequest request)
    {
        return MarkStoryBeatReferenceAsync(
            TakeDecisionOptionOperation,
            campaignId,
            sessionId,
            userId => _presentationModeHub.TakeDecisionOptionAsync(
                userId,
                campaignId,
                sessionId,
                request,
                Context.ConnectionAborted),
            PresentationModeSocketClientEvents.PresentationModeDecisionTaken);
    }

    private async Task<PresentationModeStoryBeatReferenceMarkedResponse?> MarkStoryBeatReferenceAsync(
        string operation,
        Guid campaignId,
        int sessionId,
        Func<Guid, Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>>> action,
        string clientEvent = PresentationModeSocketClientEvents.PresentationModeStoryBeatReferenceMarked)
    {
        var cancellationToken = Context.ConnectionAborted;
        var userId = await GetUserIdOrAbortAsync(
            operation,
            campaignId,
            sessionId,
            cancellationToken);

        if (userId is null)
        {
            throw new HubException("The access token does not contain a valid user identifier.");
        }

        var result = await ExecutePresentationOperationAsync(
            operation,
            campaignId,
            sessionId,
            userId.Value,
            () => action(userId.Value),
            cancellationToken);

        if (result is null)
        {
            return null;
        }

        await Clients
            .Group(SocketGroupNames.CampaignSessionPresentation(campaignId, sessionId))
            .SendAsync(
                clientEvent,
                result,
                cancellationToken);

        return result;
    }

    private async Task<TResponse?> ExecutePresentationOperationAsync<TResponse>(
        string operation,
        Guid campaignId,
        int sessionId,
        Guid userId,
        Func<Task<ServiceResult<TResponse>>> action,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        ServiceResult<TResponse> result;

        try
        {
            result = await action();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Presentation mode socket operation failed. Operation: {Operation}, CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}",
                operation,
                campaignId,
                sessionId,
                userId,
                Context.ConnectionId);

            await SendErrorAsync(
                operation,
                campaignId,
                sessionId,
                "UnexpectedError",
                "An unexpected error occurred while handling presentation mode.",
                cancellationToken);

            throw new HubException("An unexpected error occurred while handling presentation mode.");
        }

        if (result.StatusCode == ApplicationStatusCode.Success && result.Data is not null)
        {
            return result.Data;
        }

        var errorMessage = ToHubErrorMessage(result.StatusCode);

        _logger.LogWarning(
            "Presentation mode socket operation rejected. Operation: {Operation}, CampaignId: {CampaignId}, SessionId: {SessionId}, UserId: {UserId}, ConnectionId: {ConnectionId}, StatusCode: {StatusCode}",
            operation,
            campaignId,
            sessionId,
            userId,
            Context.ConnectionId,
            result.StatusCode);

        await SendErrorAsync(
            operation,
            campaignId,
            sessionId,
            result.StatusCode.ToString(),
            errorMessage,
            cancellationToken);

        return null;
    }

    private async Task<Guid?> GetUserIdOrAbortAsync(
        string operation,
        Guid campaignId,
        int sessionId,
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
            sessionId,
            "InvalidUserClaim",
            "The access token does not contain a valid user identifier.",
            cancellationToken);
        Context.Abort();

        return null;
    }

    private Task SendErrorAsync(
        string operation,
        Guid? campaignId,
        int? sessionId,
        string errorCode,
        string message,
        CancellationToken cancellationToken)
    {
        return Clients.Caller.SendAsync(
            PresentationModeSocketClientEvents.PresentationModeError,
            new PresentationModeSocketErrorResponse
            {
                Operation = operation,
                ErrorCode = errorCode,
                Message = message,
                CampaignId = campaignId,
                SessionId = sessionId
            },
            cancellationToken);
    }

    private static string ToHubErrorMessage(ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidCampaignPresentation => "Presentation mode request is invalid.",
            ApplicationStatusCode.UserNotFound => "User was not found.",
            ApplicationStatusCode.CampaignNotFound => "Campaign was not found.",
            ApplicationStatusCode.CampaignSessionNotFound => "Campaign session was not found.",
            ApplicationStatusCode.CampaignPresentationNotFound => "Presentation mode has not been initiated for this session.",
            ApplicationStatusCode.InvalidSessionNote => "Session note is invalid.",
            ApplicationStatusCode.StoryBlockNotFound => "Story block was not found.",
            ApplicationStatusCode.StoryBeatNotFound => "Story beat was not found.",
            ApplicationStatusCode.CampaignPresentationStoryBeatConflict => "Another story beat has already been selected for this story beat index.",
            ApplicationStatusCode.CampaignMasterRoleRequired => "Only users with the Master role can manage presentation mode.",
            _ => "An unexpected error occurred."
        };
    }
}
