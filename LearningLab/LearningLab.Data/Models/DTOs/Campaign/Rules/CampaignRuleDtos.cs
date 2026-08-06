using LearningLab.Data.Models.Campaign.Rules;

namespace LearningLab.Data.Models.DTOs.Campaign.Rules;

public sealed class CampaignEventDefinitionRequest
{
    public string? Key { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public CampaignEventType EventType { get; init; }
    public bool IsRepeatable { get; init; }
}

public sealed class CampaignEventDefinitionResponse
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public required string Key { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public CampaignEventType EventType { get; init; }
    public bool IsRepeatable { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public CampaignEventStateResponse? CurrentState { get; init; }
    public IReadOnlyList<CampaignEventOptionResponse> Options { get; init; } = [];
}

public sealed class CampaignEventOptionRequest
{
    public string? Key { get; init; }
    public string? Label { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CampaignEventOptionResponse
{
    public Guid Id { get; init; }
    public Guid CampaignEventDefinitionId { get; init; }
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CampaignEventStateRequest
{
    public bool? BooleanValue { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public Guid? SourceStoryBlockId { get; init; }
    public Guid? SourceStoryBeatId { get; init; }
}

public sealed class CampaignEventStateResponse
{
    public Guid Id { get; init; }
    public int CampaignSessionId { get; init; }
    public Guid CampaignEventDefinitionId { get; init; }
    public required string EventKey { get; init; }
    public CampaignEventType EventType { get; init; }
    public bool? BooleanValue { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? SelectedOptionKey { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public Guid? SourceStoryBlockId { get; init; }
    public Guid? SourceStoryBeatId { get; init; }
    public DateTimeOffset ResolvedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed class ConditionalRuleRequest
{
    public ConditionalRuleEffectType EffectType { get; init; }
    public ConditionalTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public ConditionGroupRequest? Root { get; init; }
}

public sealed class ConditionalRuleResponse
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public ConditionalTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public ConditionalRuleEffectType EffectType { get; init; }
    public ConditionGroupResponse? Root { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed class ConditionGroupRequest
{
    public ConditionGroupOperator Operator { get; init; }
    public bool Negate { get; init; }
    public IReadOnlyList<ConditionClauseRequest> Clauses { get; init; } = [];
    public IReadOnlyList<ConditionGroupRequest> Groups { get; init; } = [];
}

public sealed class ConditionGroupResponse
{
    public Guid Id { get; init; }
    public ConditionGroupOperator Operator { get; init; }
    public bool Negate { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyList<ConditionClauseResponse> Clauses { get; init; } = [];
    public IReadOnlyList<ConditionGroupResponse> Groups { get; init; } = [];
}

public sealed class ConditionClauseRequest
{
    public Guid EventDefinitionId { get; init; }
    public ConditionComparisonOperator ComparisonOperator { get; init; }
    public bool? BooleanValue { get; init; }
    public Guid? ExpectedOptionId { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
}

public sealed class ConditionClauseResponse
{
    public Guid Id { get; init; }
    public Guid EventDefinitionId { get; init; }
    public required string EventKey { get; init; }
    public ConditionComparisonOperator ComparisonOperator { get; init; }
    public bool? BooleanValue { get; init; }
    public Guid? ExpectedOptionId { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public int SortOrder { get; init; }
}

public sealed class RuleEvaluationRequest
{
    public Guid? RuleId { get; init; }
    public ConditionalTargetType? TargetType { get; init; }
    public Guid? TargetId { get; init; }
}

public sealed class RuleEvaluationResult
{
    public Guid RuleId { get; init; }
    public bool IsSatisfied { get; init; }
    public EvaluatedConditionGroupResponse? EvaluatedGroup { get; init; }
    public IReadOnlyList<EvaluatedConditionClauseResponse> FailedClauses { get; init; } = [];
    public IReadOnlyList<MissingEventResponse> MissingEvents { get; init; } = [];
    public required string HumanReadableExplanation { get; init; }
}

public sealed class EvaluatedConditionGroupResponse
{
    public Guid GroupId { get; init; }
    public ConditionGroupOperator Operator { get; init; }
    public bool Negate { get; init; }
    public bool IsSatisfied { get; init; }
    public IReadOnlyList<EvaluatedConditionClauseResponse> Clauses { get; init; } = [];
    public IReadOnlyList<EvaluatedConditionGroupResponse> Groups { get; init; } = [];
}

public sealed class EvaluatedConditionClauseResponse
{
    public Guid ClauseId { get; init; }
    public Guid EventDefinitionId { get; init; }
    public required string EventKey { get; init; }
    public ConditionComparisonOperator ComparisonOperator { get; init; }
    public bool IsSatisfied { get; init; }
    public required string Explanation { get; init; }
}

public sealed class MissingEventResponse
{
    public Guid EventDefinitionId { get; init; }
    public required string EventKey { get; init; }
}

public sealed class TargetAvailabilityResult
{
    public ConditionalTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsAvailableByRule { get; init; }
    public IReadOnlyList<RuleEvaluationResult> SatisfiedRuleResults { get; init; } = [];
    public IReadOnlyList<RuleEvaluationResult> BlockingRuleResults { get; init; } = [];
    public IReadOnlyList<RuleEvaluationResult> RuleResults { get; init; } = [];
}

public sealed class StoryOutcomeEffectRequest
{
    public OutcomeSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
    public Guid EventDefinitionId { get; init; }
    public OutcomeOperationType OperationType { get; init; }
    public bool? BooleanValue { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public int SortOrder { get; init; }
}

public sealed class StoryOutcomeEffectResponse
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public OutcomeSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
    public Guid EventDefinitionId { get; init; }
    public required string EventKey { get; init; }
    public OutcomeOperationType OperationType { get; init; }
    public bool? BooleanValue { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? SelectedOptionKey { get; init; }
    public string? TextValue { get; init; }
    public decimal? NumericValue { get; init; }
    public int SortOrder { get; init; }
}

public sealed class ApplyOutcomeRequest
{
    public OutcomeSourceType SourceType { get; init; }
    public Guid SourceId { get; init; }
}

public sealed class ApplyOutcomeResult
{
    public IReadOnlyList<CampaignEventStateResponse> ChangedEventStates { get; init; } = [];
}

public sealed class CampaignChoiceDefinitionRequest
{
    public Guid? StoryBlockId { get; init; }
    public Guid? StoryBeatId { get; init; }
    public string? Name { get; init; }
    public CampaignChoiceSelectionMode SelectionMode { get; init; }
}

public sealed class CampaignChoiceDefinitionResponse
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public Guid? StoryBlockId { get; init; }
    public Guid? StoryBeatId { get; init; }
    public required string Name { get; init; }
    public CampaignChoiceSelectionMode SelectionMode { get; init; }
    public IReadOnlyList<CampaignChoiceOptionResponse> Options { get; init; } = [];
}

public sealed class CampaignChoiceOptionRequest
{
    public Guid? StoryBeatId { get; init; }
    public string? Key { get; init; }
    public string? Label { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CampaignChoiceOptionResponse
{
    public Guid Id { get; init; }
    public Guid CampaignChoiceDefinitionId { get; init; }
    public Guid? StoryBeatId { get; init; }
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

public sealed class SelectCampaignChoiceOptionRequest
{
    public Guid ChoiceOptionId { get; init; }
}

public sealed class CampaignChoiceSelectionResponse
{
    public Guid Id { get; init; }
    public int CampaignSessionId { get; init; }
    public Guid CampaignChoiceDefinitionId { get; init; }
    public Guid CampaignChoiceOptionId { get; init; }
    public DateTimeOffset SelectedAtUtc { get; init; }
}
