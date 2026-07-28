namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatDecisionOptionRequest
{
    public Guid? Id { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public bool IsSelected { get; init; }
}
