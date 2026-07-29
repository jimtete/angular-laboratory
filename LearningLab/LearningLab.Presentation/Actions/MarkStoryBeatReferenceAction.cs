using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignSessionService;

namespace LearningLab.Presentation.Actions;

public class MarkStoryBeatReferenceAction
    : PresentAction<MarkPresentationStoryBeatReferenceRequest, PresentationModeStoryBeatReferenceMarkedResponse>
{
    private readonly ICampaignSessionService _campaignSessionService;
    private readonly GetPresentationModeWorkspaceAction _getPresentationModeWorkspaceAction;

    public MarkStoryBeatReferenceAction(
        GetPresentationModeWorkspaceAction getPresentationModeWorkspaceAction,
        ICampaignSessionService campaignSessionService)
    {
        _getPresentationModeWorkspaceAction = getPresentationModeWorkspaceAction;
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

        return new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
            ApplicationStatusCode.Success,
            new PresentationModeStoryBeatReferenceMarkedResponse
            {
                Workspace = workspaceResult.Data,
                Session = sessionResult.Data
            });
    }
}
