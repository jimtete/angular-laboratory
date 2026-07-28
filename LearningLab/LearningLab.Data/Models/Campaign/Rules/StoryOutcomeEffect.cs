namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class StoryOutcomeEffect
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public OutcomeSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public Guid CampaignEventDefinitionId { get; set; }
    public CampaignEventDefinition CampaignEventDefinition { get; set; } = null!;
    public OutcomeOperationType OperationType { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public CampaignEventOption? SelectedOption { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public int SortOrder { get; set; }
}
