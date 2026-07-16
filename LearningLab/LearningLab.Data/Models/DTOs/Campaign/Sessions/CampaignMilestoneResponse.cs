using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CampaignMilestoneResponse
{
    public int Id { get; init; }

    public Guid CampaignId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? AchievedAt { get; init; }

    public CampaignMilestoneImportance Importance { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
