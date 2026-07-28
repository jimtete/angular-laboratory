namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class StoryBeatQuestTaskResponse
{
    public Guid StoryBeatId { get; init; }
    public Guid QuestTaskId { get; init; }
    public Guid QuestId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset? DateCompleted { get; init; }
    public DateTimeOffset LinkedAt { get; init; }
}
