using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class MarkPresentationStoryBeatReferenceRequest
{
    public Guid StoryBeatId { get; init; }

    public SessionNoteStoryBeatReferenceType ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }

    public string? Content { get; init; }
}
