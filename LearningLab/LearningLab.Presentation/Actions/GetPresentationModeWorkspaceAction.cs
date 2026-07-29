using LearningLab.Data.Models;
using LearningLab.Presentation.Models;
using LearningLab.Presentation.Services;

namespace LearningLab.Presentation.Actions;

public sealed class GetPresentationModeWorkspaceAction
    : PresentAction<PresentationModeWorkspaceResponse>
{
    private readonly ICampaignPresentationService _campaignPresentationService;
    private readonly PresentationModeWorkspaceBuilder _workspaceBuilder;

    public GetPresentationModeWorkspaceAction(
        ICampaignPresentationService campaignPresentationService,
        PresentationModeWorkspaceBuilder workspaceBuilder)
    {
        _campaignPresentationService = campaignPresentationService;
        _workspaceBuilder = workspaceBuilder;
    }

    public override async Task<ServiceResult<PresentationModeWorkspaceResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var presentationResult = await _campaignPresentationService.GetPresentationModeAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);

        return await _workspaceBuilder.BuildWorkspaceResponseAsync(
            userId,
            campaignId,
            presentationResult,
            cancellationToken);
    }
}
