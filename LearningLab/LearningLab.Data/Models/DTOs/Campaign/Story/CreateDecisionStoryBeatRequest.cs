namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateDecisionStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatDecisionRequest? Decision { get; init; }
}
