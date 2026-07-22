namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatDecisionOptionResponse
{
    public int OrderIndex { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public bool IsSelected { get; init; }
}
