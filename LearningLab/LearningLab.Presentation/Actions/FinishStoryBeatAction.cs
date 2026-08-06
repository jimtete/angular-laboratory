using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignRulesService;
using LearningLab.Services.CampaignSessionService;

namespace LearningLab.Presentation.Actions;

public class FinishStoryBeatAction
    : PresentAction<FinishPresentationStoryBeatRequest, PresentationModeStoryBeatPlayedResponse>
{
    private readonly ICampaignSessionService _campaignSessionService;
    private readonly ICampaignRulesService _campaignRulesService;
    private readonly GetPresentationModeWorkspaceAction _getPresentationModeWorkspaceAction;

    public FinishStoryBeatAction(
        GetPresentationModeWorkspaceAction getPresentationModeWorkspaceAction,
        ICampaignRulesService campaignRulesService,
        ICampaignSessionService campaignSessionService)
    {
        _getPresentationModeWorkspaceAction = getPresentationModeWorkspaceAction;
        _campaignRulesService = campaignRulesService;
        _campaignSessionService = campaignSessionService;
    }

    public override async Task<ServiceResult<PresentationModeStoryBeatPlayedResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        FinishPresentationStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.StoryBeatId == Guid.Empty)
        {
            return new ServiceResult<PresentationModeStoryBeatPlayedResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var sessionResult = await _campaignSessionService.CreateStoryBeatPlayedSessionNoteAsync(
            userId,
            campaignId,
            sessionId,
            request.StoryBeatId,
            request.Content,
            cancellationToken);

        if (sessionResult.StatusCode != ApplicationStatusCode.Success
            || sessionResult.Data is null)
        {
            return new ServiceResult<PresentationModeStoryBeatPlayedResponse>(
                sessionResult.StatusCode);
        }

        var applyResult = await _campaignRulesService.ApplyOutcomeEffectsAsync(
            userId,
            sessionId,
            OutcomeSourceType.StoryBeat,
            request.StoryBeatId,
            cancellationToken);

        if (applyResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBeatPlayedResponse>(
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
            return new ServiceResult<PresentationModeStoryBeatPlayedResponse>(
                workspaceResult.StatusCode);
        }

        return new ServiceResult<PresentationModeStoryBeatPlayedResponse>(
            ApplicationStatusCode.Success,
            new PresentationModeStoryBeatPlayedResponse
            {
                Workspace = workspaceResult.Data,
                Session = sessionResult.Data,
                ChangedEventStates = applyResult.Data?.ChangedEventStates ?? []
            });
    }
}
