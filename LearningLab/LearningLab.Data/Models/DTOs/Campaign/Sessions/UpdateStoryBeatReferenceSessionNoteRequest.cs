using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class UpdateStoryBeatReferenceSessionNoteRequest
{
    public Guid StoryBeatId { get; init; }

    public SessionNoteStoryBeatReferenceType ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }

    public bool IsPlayed { get; init; }

    public string? Content { get; init; }
}
