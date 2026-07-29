namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class ReorderStoryBeatsRequest
{
    public IReadOnlyList<ReorderStoryBeatRequest> StoryBeats { get; init; } = [];
}

public sealed class ReorderStoryBeatRequest
{
    public Guid StoryBeatId { get; init; }

    public int OrderIndex { get; init; }

    public int SecondaryOrderIndex { get; init; }
}
