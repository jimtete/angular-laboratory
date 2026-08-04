using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Rules;
using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Presentation.Models;

public sealed class PresentationModeStoryBlockResponse
{
    public required StoryBlockResponse StoryBlock { get; init; }

    public IReadOnlyList<StoryBeatResponse> StoryBeats { get; init; } = [];

    public IReadOnlyList<PresentationModeStoryBeatAvailabilityResponse> StoryBeatAvailability { get; init; } = [];

    public IReadOnlyList<PresentationModeStoryBeatChoiceGroupResponse> IndexPathChoiceGroups { get; init; } = [];

    public IReadOnlyList<CampaignQuestResponse> Quests { get; init; } = [];

    public IReadOnlyList<StoryBeatQuestTaskResponse> StoryBeatQuestTaskLinks { get; init; } = [];
}

public sealed class PresentationModeStoryBeatChoiceGroupResponse
{
    public int OrderIndex { get; init; }

    public StoryBeatIndexPathRuleResponse? IndexPathRule { get; init; }

    public IReadOnlyList<StoryBeatResponse> StoryBeats { get; init; } = [];
}

public sealed class PresentationModeStoryBeatAvailabilityResponse
{
    public Guid StoryBeatId { get; init; }

    public bool IsAvailable { get; init; } = true;

    public IReadOnlyList<PresentationModeBlockingEventResponse> BlockingEvents { get; init; } = [];

    public TargetAvailabilityResult? Availability { get; init; }
}

public sealed class PresentationModeBlockingEventResponse
{
    public Guid RuleId { get; init; }

    public Guid EventDefinitionId { get; init; }

    public required string EventKey { get; init; }

    public Guid? ClauseId { get; init; }

    public bool IsMissing { get; init; }

    public required string Explanation { get; init; }
}
