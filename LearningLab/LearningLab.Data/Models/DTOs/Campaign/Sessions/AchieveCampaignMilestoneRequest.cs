namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class AchieveCampaignMilestoneRequest
{
    public int MilestoneId { get; init; }

    public string? Content { get; init; }
}
