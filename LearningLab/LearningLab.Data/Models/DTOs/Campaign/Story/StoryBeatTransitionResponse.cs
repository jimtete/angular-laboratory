namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatTransitionResponse
{
    public required string Description { get; init; }

    public IReadOnlyList<StoryBeatTransitionConclusionResponse> Conclusions { get; init; } = [];
}
