namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class TakePresentationDecisionOptionRequest
{
    public Guid StoryBeatId { get; init; }

    public Guid DecisionOptionId { get; init; }

    public string? Content { get; init; }
}
