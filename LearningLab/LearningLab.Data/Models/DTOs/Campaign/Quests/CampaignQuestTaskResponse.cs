namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class CampaignQuestTaskResponse
{
    public Guid QuestTaskId { get; init; }

    public Guid QuestId { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public DateTimeOffset? DateCompleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
