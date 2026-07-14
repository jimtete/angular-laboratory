namespace LearningLab.Data.Models.Campaign;

public class CampaignSettings
{
    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int MaxNumberOfPlayers { get; set; }
}
