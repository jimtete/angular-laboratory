using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;

namespace LearningLab.Presentation.Actions;

public sealed class FinishRoleplayingBeatAction
    : PresentAction<MarkPresentationStoryBeatReferenceRequest, PresentationModeStoryBeatReferenceMarkedResponse>
{
    private readonly MarkStoryBeatReferenceAction _markStoryBeatReferenceAction;

    public FinishRoleplayingBeatAction(MarkStoryBeatReferenceAction markStoryBeatReferenceAction)
    {
        _markStoryBeatReferenceAction = markStoryBeatReferenceAction;
    }

    public override Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationStoryBeatReferenceRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null
            || request.ReferenceType is not SessionNoteStoryBeatReferenceType.RoleplayingInformation
                and not SessionNoteStoryBeatReferenceType.RoleplayingNpcInteraction)
        {
            return Task.FromResult(new ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>(
                ApplicationStatusCode.InvalidCampaignPresentation));
        }

        return _markStoryBeatReferenceAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }
}
