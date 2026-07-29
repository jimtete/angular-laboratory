namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CreateStoryBeatPlayedSessionNoteRequest
{
    public Guid StoryBeatId { get; init; }

    public string? Content { get; init; }
}
