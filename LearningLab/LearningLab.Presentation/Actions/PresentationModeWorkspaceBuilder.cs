using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Rules;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Presentation.Models;
using LearningLab.Services.CampaignQuestService;
using LearningLab.Services.CampaignRulesService;
using LearningLab.Services.CampaignStoryService;

namespace LearningLab.Presentation.Actions;

public sealed class PresentationModeWorkspaceBuilder
{
    private readonly ICampaignQuestService _campaignQuestService;
    private readonly ICampaignRulesService _campaignRulesService;
    private readonly ICampaignStoryService _campaignStoryService;

    public PresentationModeWorkspaceBuilder(
        ICampaignQuestService campaignQuestService,
        ICampaignRulesService campaignRulesService,
        ICampaignStoryService campaignStoryService)
    {
        _campaignQuestService = campaignQuestService;
        _campaignRulesService = campaignRulesService;
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

        var outcomeEffectsResult = await _campaignRulesService.GetCampaignOutcomeEffectsAsync(
            userId,
            campaignId,
            cancellationToken);

        if (outcomeEffectsResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeWorkspaceResponse>(
                outcomeEffectsResult.StatusCode);
        }

        var storyBlocks = new List<PresentationModeStoryBlockResponse>();
        var quests = questsResult.Data ?? [];
        var questTaskLinks = questTaskLinksResult.Data ?? [];
        var outcomeEffects = outcomeEffectsResult.Data ?? [];

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

            var availabilityResult = await EvaluateStoryBeatAvailabilityAsync(
                userId,
                presentationResult.Data.CampaignSessionId,
                storyBeatsResult.Data ?? [],
                cancellationToken);

            if (availabilityResult.StatusCode != ApplicationStatusCode.Success)
            {
                return new ServiceResult<PresentationModeWorkspaceResponse>(
                    availabilityResult.StatusCode);
            }

            storyBlocks.Add(BuildStoryBlockResponse(
                storyBlock,
                storyBeatsResult.Data ?? [],
                availabilityResult.Data ?? [],
                quests,
                questTaskLinks,
                outcomeEffects));
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
        int sessionId,
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

        var availabilityResult = await EvaluateStoryBeatAvailabilityAsync(
            userId,
            sessionId,
            storyBeatsResult.Data ?? [],
            cancellationToken);

        if (availabilityResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                availabilityResult.StatusCode);
        }

        var outcomeEffectsResult = await _campaignRulesService.GetCampaignOutcomeEffectsAsync(
            userId,
            campaignId,
            cancellationToken);

        if (outcomeEffectsResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<PresentationModeStoryBlockResponse>(
                outcomeEffectsResult.StatusCode);
        }

        return new ServiceResult<PresentationModeStoryBlockResponse>(
            ApplicationStatusCode.Success,
            BuildStoryBlockResponse(
                storyBlock,
                storyBeatsResult.Data ?? [],
                availabilityResult.Data ?? [],
                questsResult.Data ?? [],
                questTaskLinksResult.Data ?? [],
                outcomeEffectsResult.Data ?? []));
    }

    private async Task<ServiceResult<IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse>>>
        EvaluateStoryBeatAvailabilityAsync(
            Guid userId,
            int sessionId,
            IReadOnlyList<StoryBeatResponse> storyBeats,
            CancellationToken cancellationToken)
    {
        if (storyBeats.Count == 0)
        {
            return new ServiceResult<IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse>>(
                ApplicationStatusCode.Success,
                []);
        }

        var availabilityResult = await _campaignRulesService.EvaluateTargetsAsync(
            userId,
            ConditionalTargetType.StoryBeat,
            storyBeats.Select(beat => beat.StoryBeatId).ToList(),
            sessionId,
            cancellationToken);

        return availabilityResult.StatusCode != ApplicationStatusCode.Success
            ? new ServiceResult<IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse>>(
                availabilityResult.StatusCode)
            : new ServiceResult<IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse>>(
                ApplicationStatusCode.Success,
                (availabilityResult.Data ?? [])
                    .Select(ToAvailabilityResponse)
                    .ToList());
    }

    private static PresentationModeStoryBlockResponse BuildStoryBlockResponse(
        StoryBlockResponse storyBlock,
        IReadOnlyList<StoryBeatResponse> storyBeats,
        IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse> availability,
        IReadOnlyList<CampaignQuestResponse> quests,
        IReadOnlyList<StoryBeatQuestTaskResponse> questTaskLinks,
        IReadOnlyList<StoryOutcomeEffectResponse> outcomeEffects)
    {
        var orderedStoryBeats = storyBeats
            .OrderBy(beat => beat.OrderIndex)
            .ThenBy(beat => beat.SecondaryOrderIndex)
            .ThenBy(beat => beat.StoryBeatId)
            .ToList();
        var pendingOutcomeEffectsByStoryBeatId = BuildPendingOutcomeEffects(
                orderedStoryBeats,
                outcomeEffects)
            .ToLookup(effect => effect.StoryBeatId);
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
            StoryBeatAvailability = availability
                .Select(item => WithPendingOutcomeEffects(
                    item,
                    pendingOutcomeEffectsByStoryBeatId[item.StoryBeatId].ToList()))
                .ToList(),
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

    private static IReadOnlyList<PresentationModePendingOutcomeEffectResponse> BuildPendingOutcomeEffects(
        IReadOnlyList<StoryBeatResponse> storyBeats,
        IReadOnlyList<StoryOutcomeEffectResponse> outcomeEffects)
    {
        var effectsBySource = outcomeEffects
            .ToLookup(effect => (effect.SourceType, effect.SourceId));

        return storyBeats
            .SelectMany(beat => BuildPendingOutcomeEffectSources(beat)
                .Select(source => new PresentationModePendingOutcomeEffectResponse
                {
                    StoryBeatId = beat.StoryBeatId,
                    SourceType = source.SourceType,
                    SourceId = source.SourceId,
                    Effects = effectsBySource[(source.SourceType, source.SourceId)]
                        .OrderBy(effect => effect.SortOrder)
                        .ToList()
                }))
            .Where(source => source.Effects.Count > 0)
            .ToList();
    }

    private static IEnumerable<(OutcomeSourceType SourceType, Guid SourceId)> BuildPendingOutcomeEffectSources(
        StoryBeatResponse storyBeat)
    {
        yield return (OutcomeSourceType.StoryBeat, storyBeat.StoryBeatId);

        foreach (var decision in storyBeat.Decision?.Decisions ?? [])
        {
            yield return (OutcomeSourceType.DecisionChoice, decision.Id);
        }

        foreach (var npcReference in storyBeat.Roleplaying?.NpcReferences ?? [])
        {
            yield return (OutcomeSourceType.RoleplayingNpcInteraction, npcReference.Id);
        }

        foreach (var information in storyBeat.Roleplaying?.DiscoverableInformation ?? [])
        {
            yield return (OutcomeSourceType.RoleplayingInformation, information.Id);
        }
    }

    private static PresentationModeStoryBeatAvailabilityResponse WithPendingOutcomeEffects(
        PresentationModeStoryBeatAvailabilityResponse availability,
        IReadOnlyList<PresentationModePendingOutcomeEffectResponse> pendingOutcomeEffects)
    {
        return new PresentationModeStoryBeatAvailabilityResponse
        {
            StoryBeatId = availability.StoryBeatId,
            IsAvailable = availability.IsAvailable,
            IsAvailableByRule = availability.IsAvailableByRule,
            SatisfiedRules = availability.SatisfiedRules,
            BlockingEvents = availability.BlockingEvents,
            PendingOutcomeEffects = pendingOutcomeEffects,
            Availability = availability.Availability
        };
    }

    private static PresentationModeStoryBeatAvailabilityResponse ToAvailabilityResponse(
        TargetAvailabilityResult availability)
    {
        return new PresentationModeStoryBeatAvailabilityResponse
        {
            StoryBeatId = availability.TargetId,
            IsAvailable = availability.IsAvailable,
            IsAvailableByRule = availability.IsAvailableByRule,
            SatisfiedRules = BuildSatisfiedRules(availability),
            BlockingEvents = availability.IsAvailable
                ? []
                : BuildBlockingEvents(availability),
            Availability = availability
        };
    }

    private static IReadOnlyList<PresentationModeSatisfiedRuleResponse> BuildSatisfiedRules(
        TargetAvailabilityResult availability)
    {
        return availability.SatisfiedRuleResults
            .Select(rule => new PresentationModeSatisfiedRuleResponse
            {
                RuleId = rule.RuleId,
                Explanation = rule.HumanReadableExplanation
            })
            .ToList();
    }

    private static IReadOnlyList<PresentationModeBlockingEventResponse> BuildBlockingEvents(
        TargetAvailabilityResult availability)
    {
        return availability.RuleResults
            .Where(rule => !rule.IsSatisfied)
            .SelectMany(rule =>
                rule.MissingEvents
                    .Select(missingEvent => new PresentationModeBlockingEventResponse
                    {
                        RuleId = rule.RuleId,
                        EventDefinitionId = missingEvent.EventDefinitionId,
                        EventKey = missingEvent.EventKey,
                        ClauseId = null,
                        IsMissing = true,
                        Explanation = $"Event '{missingEvent.EventKey}' has not been set."
                    })
                    .Concat(rule.FailedClauses.Select(clause => new PresentationModeBlockingEventResponse
                    {
                        RuleId = rule.RuleId,
                        EventDefinitionId = clause.EventDefinitionId,
                        EventKey = clause.EventKey,
                        ClauseId = clause.ClauseId,
                        IsMissing = false,
                        Explanation = clause.Explanation
                    })))
            .GroupBy(blocker => new
            {
                blocker.RuleId,
                blocker.EventDefinitionId,
                blocker.ClauseId,
                blocker.IsMissing
            })
            .Select(group => group.First())
            .ToList();
    }
}
