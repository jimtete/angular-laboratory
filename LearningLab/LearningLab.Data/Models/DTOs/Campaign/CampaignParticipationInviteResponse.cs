namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignParticipationInviteResponse
{
    public Guid CampaignId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public DateTimeOffset DateInvited { get; set; }
}
