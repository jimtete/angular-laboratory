using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignQuestService;
using LearningLab.Services.CampaignStoryService;

namespace LearningLab.Presentation.Actions;

public sealed class PresentationModeWorkspaceBuilder
{
    private readonly ICampaignQuestService _campaignQuestService;
    private readonly ICampaignStoryService _campaignStoryService;

    public PresentationModeWorkspaceBuilder(
        ICampaignQuestService campaignQuestService,
        ICampaignStoryService campaignStoryService)
    {
        _campaignQuestService = campaignQuestService;
        _campaignStoryService = campaignStoryService;
    }

    public async Task<ServiceResult<PresentationModeWorkspaceResponse>> BuildWorkspaceResponseAsync(
        Guid userId,
        Guid campaignId,
        ServiceResult<CampaignPresentationResponse> presentationResult,
        CancellationToken cancellationToken = default)
    {
        if (presentationResult.StatusCode != ApplicationStatusCode.Success
            || presentationResult.Data is null)
        {
            return new ServiceResult<PresentationModeWorkspaceResponse>(
                presentationResult.StatusCode);
        }

        var storyBlocksResult = await _campaignStoryService.GetStoryBlocksAsync(
            userId,
            campaignId,
            cancellationToken);

        if (storyBlocksResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeWorkspaceResponse>(
                storyBlocksResult.StatusCode);
        }

        var questsResult = await _campaignQuestService.GetCampaignQuestsAsync(
            userId,
            campaignId,
            cancellationToken);

        if (questsResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeWorkspaceResponse>(
                questsResult.StatusCode);
        }

        var questTaskLinksResult = await _campaignQuestService.GetCampaignStoryBeatQuestTasksAsync(
            userId,
            campaignId,
            cancellationToken);

        if (questTaskLinksResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeWorkspaceResponse>(
                questTaskLinksResult.StatusCode);
        }

        var storyBlocks = new List<PresentationModeStoryBlockResponse>();
        var quests = questsResult.Data ?? [];
        var questTaskLinks = questTaskLinksResult.Data ?? [];

        foreach (var storyBlock in storyBlocksResult.Data ?? [])
        {
            var storyBeatsResult = await _campaignStoryService.GetStoryBeatsAsync(
                userId,
                campaignId,
                storyBlock.StoryBlockId,
                cancellationToken);

            if (storyBeatsResult.StatusCode != ApplicationStatusCode.Success)
            {
                return new ServiceResult<PresentationModeWorkspaceResponse>(
                    storyBeatsResult.StatusCode);
            }

            storyBlocks.Add(BuildStoryBlockResponse(
                storyBlock,
                storyBeatsResult.Data ?? [],
                quests,
                questTaskLinks));
        }

        return new ServiceResult<PresentationModeWorkspaceResponse>(
            ApplicationStatusCode.Success,
            new PresentationModeWorkspaceResponse
            {
                Presentation = presentationResult.Data,
                StoryBlocks = storyBlocks,
                Quests = quests,
                StoryBeatQuestTaskLinks = questTaskLinks
            });
    }

    public async Task<ServiceResult<PresentationModeStoryBlockResponse>> BuildStoryBlockResponseAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        if (storyBlockId == Guid.Empty)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBlocksResult = await _campaignStoryService.GetStoryBlocksAsync(
            userId,
            campaignId,
            cancellationToken);

        if (storyBlocksResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                storyBlocksResult.StatusCode);
        }

        var storyBlock = (storyBlocksResult.Data ?? [])
            .SingleOrDefault(block => block.StoryBlockId == storyBlockId);

        if (storyBlock is null)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                ApplicationStatusCode.StoryBlockNotFound);
        }

        var storyBeatsResult = await _campaignStoryService.GetStoryBeatsAsync(
            userId,
            campaignId,
            storyBlockId,
            cancellationToken);

        if (storyBeatsResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                storyBeatsResult.StatusCode);
        }

        var questsResult = await _campaignQuestService.GetCampaignQuestsAsync(
            userId,
            campaignId,
            cancellationToken);

        if (questsResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                questsResult.StatusCode);
        }

        var questTaskLinksResult = await _campaignQuestService.GetCampaignStoryBeatQuestTasksAsync(
            userId,
            campaignId,
            cancellationToken);

        if (questTaskLinksResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                questTaskLinksResult.StatusCode);
        }

        return new ServiceResult<PresentationModeStoryBlockResponse>(
            ApplicationStatusCode.Success,
            BuildStoryBlockResponse(
                storyBlock,
                storyBeatsResult.Data ?? [],
                questsResult.Data ?? [],
                questTaskLinksResult.Data ?? []));
    }

    private static PresentationModeStoryBlockResponse BuildStoryBlockResponse(
        StoryBlockResponse storyBlock,
        IReadOnlyList<StoryBeatResponse> storyBeats,
        IReadOnlyList<CampaignQuestResponse> quests,
        IReadOnlyList<StoryBeatQuestTaskResponse> questTaskLinks)
    {
        var orderedStoryBeats = storyBeats
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.StoryBeatId)
            .ToList();
        var storyBeatIds = storyBeats
            .Select(beat => beat.StoryBeatId)
            .ToHashSet();
        var blockQuestTaskLinks = questTaskLinks
            .Where(link => storyBeatIds.Contains(link.StoryBeatId))
            .ToList();
        var questIds = blockQuestTaskLinks
            .Select(link => link.QuestId)
            .ToHashSet();

        return new PresentationModeStoryBlockResponse
        {
            StoryBlock = storyBlock,
            StoryBeats = orderedStoryBeats,
            IndexPathChoiceGroups = BuildIndexPathChoiceGroups(orderedStoryBeats),
            Quests = quests
                .Where(quest => questIds.Contains(quest.QuestId))
                .ToList(),
            StoryBeatQuestTaskLinks = blockQuestTaskLinks
        };
    }

    private static IReadOnlyList<PresentationModeStoryBeatChoiceGroupResponse> BuildIndexPathChoiceGroups(
        IReadOnlyList<StoryBeatResponse> storyBeats)
    {
        return storyBeats
            .GroupBy(beat => beat.OrderIndex)
            .Where(group => group.Count() > 1)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var groupedStoryBeats = group
                    .OrderBy(beat => beat.SecondaryOrderIndex)
                    .ThenBy(beat => beat.StoryBeatId)
                    .ToList();

                return new PresentationModeStoryBeatChoiceGroupResponse
                {
                    OrderIndex = group.Key,
                    IndexPathRule = groupedStoryBeats
                        .Select(beat => beat.IndexPathRule)
                        .FirstOrDefault(rule => rule is not null),
                    StoryBeats = groupedStoryBeats
                };
            })
            .ToList();
    }
}
