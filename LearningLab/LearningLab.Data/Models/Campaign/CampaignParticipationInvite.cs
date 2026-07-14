using LearningLab.Data.Models;

namespace LearningLab.Data.Models.Campaign;

public class CampaignParticipationInvite
{
    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTimeOffset DateInvited { get; set; }
}
