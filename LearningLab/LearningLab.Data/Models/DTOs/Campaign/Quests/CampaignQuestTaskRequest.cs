namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class CampaignQuestTaskRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? DateCompleted { get; init; }
}
