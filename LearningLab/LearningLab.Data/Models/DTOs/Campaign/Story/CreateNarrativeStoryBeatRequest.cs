namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateNarrativeStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatNarrativeRequest? Narrative { get; init; }
}
