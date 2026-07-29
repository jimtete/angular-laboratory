using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;

namespace LearningLab.Presentation.Actions;

public sealed class FinishNarrationBeatAction
    : PresentAction<FinishPresentationStoryBeatRequest, PresentationModeStoryBeatPlayedResponse>
{
    private readonly FinishStoryBeatAction _finishStoryBeatAction;

    public FinishNarrationBeatAction(FinishStoryBeatAction finishStoryBeatAction)
    {
        _finishStoryBeatAction = finishStoryBeatAction;
    }

    public override Task<ServiceResult<PresentationModeStoryBeatPlayedResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        FinishPresentationStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        return _finishStoryBeatAction.ExecuteAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);
    }
}
