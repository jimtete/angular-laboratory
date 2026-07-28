namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class ConditionalRule
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public ConditionalTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public Guid RootConditionGroupId { get; set; }
    public ConditionGroup RootConditionGroup { get; set; } = null!;
    public ConditionalRuleEffectType EffectType { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
