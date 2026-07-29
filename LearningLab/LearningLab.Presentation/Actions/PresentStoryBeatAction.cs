using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;
using LearningLab.Presentation.Services;

namespace LearningLab.Presentation.Actions;

public sealed class PresentStoryBeatAction
    : PresentAction<PresentStoryBeatRequest, PresentationModeWorkspaceResponse>
{
    private readonly ICampaignPresentationService _campaignPresentationService;
    private readonly PresentationModeWorkspaceBuilder _workspaceBuilder;

    public PresentStoryBeatAction(
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
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default)
    {
        var presentationResult = await _campaignPresentationService.PresentStoryBeatAsync(
            userId,
            campaignId,
            sessionId,
            request,
            cancellationToken);

        return await _workspaceBuilder.BuildWorkspaceResponseAsync(
            userId,
            campaignId,
            presentationResult,
            cancellationToken);
    }
}
