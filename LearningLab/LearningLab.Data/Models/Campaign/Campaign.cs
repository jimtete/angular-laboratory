using LearningLab.Data.Models;

namespace LearningLab.Data.Models.Campaign;

public class Campaign
{
    public Guid CampaignId { get; set; }
    public Guid GameMasterId { get; set; }
    public User GameMaster { get; set; } = null!;
    public string CampaignName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? CampaignPictureUrl { get; set; }
}
