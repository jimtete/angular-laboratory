namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignInviteResolutionResponse
{
    public Guid CampaignId { get; set; }

    public Guid UserId { get; set; }

    public bool Accepted { get; set; }

    public DateTimeOffset DateResolved { get; set; }
}
