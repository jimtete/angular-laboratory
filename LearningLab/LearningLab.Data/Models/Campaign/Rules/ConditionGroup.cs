namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class ConditionGroup
{
    public Guid Id { get; set; }
    public Guid? ParentConditionGroupId { get; set; }
    public ConditionGroup? ParentConditionGroup { get; set; }
    public ConditionGroupOperator Operator { get; set; }
    public bool Negate { get; set; }
    public int SortOrder { get; set; }
    public ICollection<ConditionGroup> Groups { get; set; } = [];
    public ICollection<ConditionClause> Clauses { get; set; } = [];
}
