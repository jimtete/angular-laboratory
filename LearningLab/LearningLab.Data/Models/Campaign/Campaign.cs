using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Quests;
namespace LearningLab.Data.Models.Campaign;

public class Campaign
{
    public Guid CampaignId { get; set; }
    public Guid GameMasterId { get; set; }
    public User GameMaster { get; set; } = null!;
    public string CampaignName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? CampaignPictureUrl { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public CampaignSettings Settings { get; set; } = null!;
    public List<PlayerCampaignParticipation> PlayerParticipations { get; set; } = [];
    public List<CampaignParticipationInvite> ParticipationInvites { get; set; } = [];
    public List<CampaignMilestone> Milestones { get; set; } = [];
    public List<CampaignQuest> Quests { get; set; } = [];
}
