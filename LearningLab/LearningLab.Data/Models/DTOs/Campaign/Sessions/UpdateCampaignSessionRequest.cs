namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class UpdateCampaignSessionRequest
{
    public string? Description { get; init; }

    public DateTimeOffset? SessionDate { get; init; }
}
