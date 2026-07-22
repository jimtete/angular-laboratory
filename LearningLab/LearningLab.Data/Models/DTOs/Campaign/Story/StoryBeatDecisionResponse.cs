namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatDecisionResponse
{
    public required string Description { get; init; }

    public IReadOnlyList<StoryBeatDecisionOptionResponse> Decisions { get; init; } = [];
}
