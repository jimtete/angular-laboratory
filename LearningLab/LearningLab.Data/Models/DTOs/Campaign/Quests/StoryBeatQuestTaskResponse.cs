namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class StoryBeatQuestTaskResponse
{
    public Guid StoryBeatId { get; init; }
    public Guid StoryBlockId { get; init; }
    public Guid QuestTaskId { get; init; }
    public Guid QuestId { get; init; }
    public required string StoryBeatTitle { get; init; }
    public required string StoryBlockTitle { get; init; }
    public int StoryBlockOrderIndex { get; init; }
    public int StoryBeatOrderIndex { get; init; }
    public int StoryBeatSecondaryOrderIndex { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset? DateCompleted { get; init; }
    public DateTimeOffset LinkedAt { get; init; }
}
