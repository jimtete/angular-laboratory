namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class MarkPresentationRoleplayingInformationRequest
{
    public Guid StoryBeatId { get; init; }

    public Guid InformationId { get; init; }

    public string? Content { get; init; }
}
