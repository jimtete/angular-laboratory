using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatTransitionConclusionResponse
{
    public Guid SourceStoryBeatId { get; init; }

    public required string SourceTitle { get; init; }

    public StoryBeatType SourceStoryBeatType { get; init; }

    public required string Category { get; init; }

    public required string Text { get; init; }
}
