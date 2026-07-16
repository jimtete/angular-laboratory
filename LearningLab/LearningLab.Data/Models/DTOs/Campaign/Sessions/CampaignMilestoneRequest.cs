using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CampaignMilestoneRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? AchievedAt { get; init; }

    public CampaignMilestoneImportance Importance { get; init; }
}
