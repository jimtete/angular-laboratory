namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateTransitionStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatTransitionRequest? Transition { get; init; }
}
