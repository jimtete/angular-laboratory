namespace LearningLab.Data.Models.Campaign.Maps;

public class MapCampaign
{
    public int MapId { get; set; }

    public Map Map { get; set; } = null!;

    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public DateTimeOffset DateAdded { get; set; }
}
