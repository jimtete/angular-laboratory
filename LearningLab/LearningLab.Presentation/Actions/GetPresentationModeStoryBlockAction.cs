using LearningLab.Data.Models;
using LearningLab.Presentation.Models;
using LearningLab.Presentation.Services;

namespace LearningLab.Presentation.Actions;

public sealed class GetPresentationModeStoryBlockAction
{
    private readonly ICampaignPresentationService _campaignPresentationService;
    private readonly PresentationModeWorkspaceBuilder _workspaceBuilder;

    public GetPresentationModeStoryBlockAction(
        ICampaignPresentationService campaignPresentationService,
        PresentationModeWorkspaceBuilder workspaceBuilder)
    {
        _campaignPresentationService = campaignPresentationService;
        _workspaceBuilder = workspaceBuilder;
    }

    public async Task<ServiceResult<PresentationModeStoryBlockResponse>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        if (storyBlockId == Guid.Empty)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var presentationResult = await _campaignPresentationService.GetPresentationModeAsync(
            userId,
            campaignId,
            sessionId,
            cancellationToken);

        if (presentationResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                presentationResult.StatusCode);
        }

        return await _workspaceBuilder.BuildStoryBlockResponseAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);
    }
}
