namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class ConditionClause
{
    public Guid Id { get; set; }
    public Guid ConditionGroupId { get; set; }
    public ConditionGroup ConditionGroup { get; set; } = null!;
    public Guid CampaignEventDefinitionId { get; set; }
    public CampaignEventDefinition CampaignEventDefinition { get; set; } = null!;
    public ConditionComparisonOperator ComparisonOperator { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? ExpectedOptionId { get; set; }
    public CampaignEventOption? ExpectedOption { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public int SortOrder { get; set; }
}
