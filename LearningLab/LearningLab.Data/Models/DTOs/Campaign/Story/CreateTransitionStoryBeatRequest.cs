namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateTransitionStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatTransitionRequest? Transition { get; init; }
}
