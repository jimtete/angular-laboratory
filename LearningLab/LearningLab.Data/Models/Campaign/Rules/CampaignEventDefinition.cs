namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignEventDefinition
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CampaignEventType EventType { get; set; }
    public bool IsRepeatable { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ICollection<CampaignEventOption> Options { get; set; } = [];
    public ICollection<CampaignEventState> States { get; set; } = [];
}
