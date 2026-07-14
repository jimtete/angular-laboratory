using LearningLab.Data.Models;

namespace LearningLab.Data.Models.Campaign;

public class PlayerCampaignParticipation
{
    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string? Nickname { get; set; }

    public DateTimeOffset DateJoined { get; set; }
}
