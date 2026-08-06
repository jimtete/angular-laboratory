using LearningLab.Data.Models.Campaign.Rules;
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

    public bool IsAvailableByRule { get; init; }

    public IReadOnlyList<PresentationModeSatisfiedRuleResponse> SatisfiedRules { get; init; } = [];

    public IReadOnlyList<PresentationModeBlockingEventResponse> BlockingEvents { get; init; } = [];

    public IReadOnlyList<PresentationModePendingOutcomeEffectResponse> PendingOutcomeEffects { get; init; } = [];

    public TargetAvailabilityResult? Availability { get; init; }
}

public sealed class PresentationModePendingOutcomeEffectResponse
{
    public Guid StoryBeatId { get; init; }

    public OutcomeSourceType SourceType { get; init; }

    public Guid SourceId { get; init; }

    public IReadOnlyList<StoryOutcomeEffectResponse> Effects { get; init; } = [];
}

public sealed class PresentationModeSatisfiedRuleResponse
{
    public Guid RuleId { get; init; }

    public required string Explanation { get; init; }
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
