using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Actions;
using LearningLab.Presentation.Models;
using LearningLab.Presentation.Services;

namespace LearningLab.Presentation.Hub;

public sealed class PresentationModeHub : IPresentationModeHub
{
    private readonly DisablePresentationModeAction _disablePresentationModeAction;
    private readonly EnablePresentationModeAction _enablePresentationModeAction;
    private readonly FinishNarrationBeatAction _finishNarrationBeatAction;
    private readonly FinishRoleplayingBeatAction _finishRoleplayingBeatAction;
    private readonly GetPresentationModeStoryBlockAction _getPresentationModeStoryBlockAction;
    private readonly GetPresentationModeWorkspaceAction _getPresentationModeWorkspaceAction;
    private readonly MarkStoryBeatReferenceAction _markStoryBeatReferenceAction;
    private readonly PresentStoryBeatAction _presentStoryBeatAction;
    private readonly TakeDecisionBeatAction _takeDecisionBeatAction;
    private readonly ICampaignPresentationService _campaignPresentationService;

    public PresentationModeHub(
        ICampaignPresentationService campaignPresentationService,
        GetPresentationModeWorkspaceAction getPresentationModeWorkspaceAction,
        GetPresentationModeStoryBlockAction getPresentationModeStoryBlockAction,
        EnablePresentationModeAction enablePresentationModeAction,
        DisablePresentationModeAction disablePresentationModeAction,
        PresentStoryBeatAction presentStoryBeatAction,
        FinishNarrationBeatAction finishNarrationBeatAction,
        MarkStoryBeatReferenceAction markStoryBeatReferenceAction,
        FinishRoleplayingBeatAction finishRoleplayingBeatAction,
        TakeDecisionBeatAction takeDecisionBeatAction)
    {
        _campaignPresentationService = campaignPresentationService;
        _getPresentationModeWorkspaceAction = getPresentationModeWorkspaceAction;
        _getPresentationModeStoryBlockAction = getPresentationModeStoryBlockAction;
        _enablePresentationModeAction = enablePresentationModeAction;
        _disablePresentationModeAction = disablePresentationModeAction;
        _presentStoryBeatAction = presentStoryBeatAction;
        _finishNarrationBeatAction = finishNarrationBeatAction;
        _markStoryBeatReferenceAction = markStoryBeatReferenceAction;
        _finishRoleplayingBeatAction = finishRoleplayingBeatAction;
        _takeDecisionBeatAction = takeDecisionBeatAction;
    }

    public Task<ServiceResult<CampaignPresentationResponse>> GetPresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _campaignPresentationService.GetPresentationModeAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignPresentationResponse>> InitiatePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _campaignPresentationService.InitiatePresentationModeAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignPresentationResponse>> DisablePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _campaignPresentationService.DisablePresentationModeAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeWorkspaceResponse>> GetPresentationModeWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _getPresentationModeWorkspaceAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBlockResponse>> GetPresentationModeStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return _getPresentationModeStoryBlockAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            storyBlockId,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeWorkspaceResponse>> EnablePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _enablePresentationModeAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeWorkspaceResponse>> DisablePresentationModeWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _disablePresentationModeAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeWorkspaceResponse>> PresentStoryBeatWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _presentStoryBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<CampaignPresentationResponse>> PresentStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _campaignPresentationService.PresentStoryBeatAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBeatPlayedResponse>> FinishStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        FinishPresentationStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _finishNarrationBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkStoryBeatReferenceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationStoryBeatReferenceRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _markStoryBeatReferenceAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkRoleplayingInformationGivenAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingInformationRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _finishRoleplayingBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request is null
                ? null
                : new MarkPresentationStoryBeatReferenceRequest
                {
                    StoryBeatId = request.StoryBeatId,
                    ReferenceType = SessionNoteStoryBeatReferenceType.RoleplayingInformation,
                    ReferenceId = request.InformationId,
                    Content = request.Content
                },
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkRoleplayingNpcInteractionGivenAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingNpcInteractionRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _finishRoleplayingBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request is null
                ? null
                : new MarkPresentationStoryBeatReferenceRequest
                {
                    StoryBeatId = request.StoryBeatId,
                    ReferenceType = SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction,
                    ReferenceId = request.NpcReferenceId,
                    Content = request.Content
                },
            cancellationToken);
    }

    public Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> TakeDecisionOptionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        TakePresentationDecisionOptionRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _takeDecisionBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }
}
