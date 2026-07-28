namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateNarrativeStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatNarrativeRequest? Narrative { get; init; }
}
