using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Data.Models.DTOs.Campaign.Rules;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignRulesService;
using LearningLab.Services.CampaignSessionService;

namespace LearningLab.Presentation.Actions;

public class MarkStoryBeatReferenceAction
    : PresentAction<MarkPresentationStoryBeatReferenceRequest, PresentationModeStoryBeatReferenceMarkedResponse>
{
    private readonly ICampaignSessionService _campaignSessionService;
    private readonly ICampaignRulesService _campaignRulesService;
    private readonly GetPresentationModeWorkspaceAction _getPresentationModeWorkspaceAction;

    public MarkStoryBeatReferenceAction(
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
        MarkPresentationStoryBeatReferenceRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || request.StoryBeatId == Guid.Empty
            || !Enum.IsDefined(request.ReferenceType))
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation);
        }

        var sessionResult = await _campaignSessionService.CreateStoryBeatReferenceSessionNoteAsync(
            userId,
            campaignId,
            sessionId,
            request.StoryBeatId,
            request.ReferenceType,
            request.ReferenceId,
            request.Content,
            cancellationToken);

        if (sessionResult.StatusCode != ApplicationStatusCode.Success
            || sessionResult.Data is null)
        {
            return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                sessionResult.StatusCode);
        }

        var sourceType = request.ReferenceType switch
        {
            SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction => OutcomeSourceType.RoleplayingNpcInteraction,
            SessionNoteStoryBeatReferenceType.RoleplayingInformation => OutcomeSourceType.RoleplayingInformation,
            SessionNoteStoryBeatReferenceType.DecisionOption => OutcomeSourceType.DecisionChoice,
            _ => (OutcomeSourceType?)null
        };

        IReadOnlyList<CampaignEventStateResponse> changedEventStates = [];

        if (sourceType is not null
            && request.ReferenceId is not null)
        {
            var applyResult = await _campaignRulesService.ApplyOutcomeEffectsAsync(
                userId,
                sessionId,
                sourceType.Value,
                request.ReferenceId.Value,
                cancellationToken);

            if (applyResult.StatusCode != ApplicationStatusCode.Success)
            {
                return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                    applyResult.StatusCode);
            }

            changedEventStates = applyResult.Data?.ChangedEventStates ?? [];
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
                ChangedEventStates = changedEventStates
            });
    }
}
