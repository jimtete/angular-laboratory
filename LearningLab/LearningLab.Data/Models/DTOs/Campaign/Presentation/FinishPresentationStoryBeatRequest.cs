namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class FinishPresentationStoryBeatRequest
{
    public Guid StoryBeatId { get; init; }

    public string? Content { get; init; }
}
