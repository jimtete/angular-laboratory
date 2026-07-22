namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateDecisionStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatDecisionRequest? Decision { get; init; }
}
