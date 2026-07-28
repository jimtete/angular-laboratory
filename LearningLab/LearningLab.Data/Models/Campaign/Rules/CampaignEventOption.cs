namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignEventOption
{
    public Guid Id { get; set; }
    public Guid CampaignEventDefinitionId { get; set; }
    public CampaignEventDefinition CampaignEventDefinition { get; set; } = null!;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
