namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class MarkPresentationRoleplayingNpcInteractionRequest
{
    public Guid StoryBeatId { get; init; }

    public Guid NpcReferenceId { get; init; }

    public string? Content { get; init; }
}
