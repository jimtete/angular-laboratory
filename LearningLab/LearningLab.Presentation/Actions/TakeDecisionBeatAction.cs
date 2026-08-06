using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignRulesService;
using LearningLab.Services.CampaignSessionService;

namespace LearningLab.Presentation.Actions;

public sealed class TakeDecisionBeatAction
    : PresentAction<TakePresentationDecisionOptionRequest, PresentationModeStoryBeatReferenceMarkedResponse>
{
    private readonly ICampaignSessionService _campaignSessionService;
    private readonly ICampaignRulesService _campaignRulesService;
    private readonly GetPresentationModeWorkspaceAction _getPresentationModeWorkspaceAction;

    public TakeDecisionBeatAction(
        GetPresentationModeWorkspaceAction getPresentationModeWorkspaceAction,
        ICampaignRulesService campaignRulesService,
        ICampaignSessionService campaignSessionService)
    {
        _getPresentationModeWorkspaceAction = getPresentationModeWorkspaceAction;
        _campaignRulesService = campaignRulesService;
        _campaignSessionService = campaignSessionService;
    }

    public override async Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        TakePresentationDecisionOptionRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || request.StoryBeatId == Guid.Empty
            || request.DecisionOptionId == Guid.Empty)
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var sessionResult = await _campaignSessionService.TakeDecisionStoryBeatOptionSessionNoteAsync(
            userId,
            campaignId,
            sessionId,
            request.StoryBeatId,
            request.DecisionOptionId,
            request.Content,
            cancellationToken);

        if (sessionResult.StatusCode != ApplicationStatusCode.Success
            || sessionResult.Data is null)
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                sessionResult.StatusCode);
        }

        var applyResult = await _campaignRulesService.ApplyOutcomeEffectsAsync(
            userId,
            sessionId,
            OutcomeSourceType.DecisionChoice,
            request.DecisionOptionId,
            cancellationToken);

        if (applyResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                applyResult.StatusCode);
        }

        var workspaceResult = await _getPresentationModeWorkspaceAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);

        if (workspaceResult.StatusCode != ApplicationStatusCode.Success
            || workspaceResult.Data is null)
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                workspaceResult.StatusCode);
        }

        return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
            ApplicationStatusCode.Success,
            new PresentationModeStoryBeatReferenceMarkedResponse
            {
                Workspace = workspaceResult.Data,
                Session = sessionResult.Data,
                ChangedEventStates = applyResult.Data?.ChangedEventStates ?? []
            });
    }
}
