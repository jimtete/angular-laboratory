using LearningLab.Data.Models.Monsters;

namespace LearningLab.Data.Models.Campaign;

public class CampaignNpcParticipation
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public int MonsterId { get; set; }
    public Monster Monster { get; set; } = null!;
}
