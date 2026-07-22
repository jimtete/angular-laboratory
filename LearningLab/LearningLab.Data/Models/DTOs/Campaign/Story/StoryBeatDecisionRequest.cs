namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatDecisionRequest
{
    public string? Description { get; init; }

    public IReadOnlyList<StoryBeatDecisionOptionRequest> Decisions { get; init; } = [];
}
