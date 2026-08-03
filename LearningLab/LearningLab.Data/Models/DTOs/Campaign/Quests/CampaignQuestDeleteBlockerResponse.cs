namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class CampaignQuestDeleteBlockerResponse
{
    public string BlockerType { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public Guid QuestTaskId { get; init; }

    public string QuestTaskTitle { get; init; } = string.Empty;

    public Guid? StoryBlockId { get; init; }

    public string? StoryBlockTitle { get; init; }

    public int? StoryBlockOrderIndex { get; init; }

    public Guid? StoryBeatId { get; init; }

    public string? StoryBeatTitle { get; init; }

    public int? StoryBeatOrderIndex { get; init; }

    public int? StoryBeatSecondaryOrderIndex { get; init; }
}
