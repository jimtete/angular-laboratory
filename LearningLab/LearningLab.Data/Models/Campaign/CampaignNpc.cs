namespace LearningLab.Data.Models.Campaign;

public class CampaignNpc
{
    public Guid CampaignNpcId { get; set; }

    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public string Tag { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
