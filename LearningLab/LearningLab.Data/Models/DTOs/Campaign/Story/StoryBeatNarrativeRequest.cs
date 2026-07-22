namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatNarrativeRequest
{
    public IReadOnlyList<string?> Paragraphs { get; init; } = [];
}
