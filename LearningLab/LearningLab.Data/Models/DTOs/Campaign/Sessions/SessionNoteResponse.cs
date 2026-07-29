using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteResponse
{
    public int Id { get; init; }

    public int SessionId { get; init; }

    public int Order { get; init; }

    public SessionNoteType Type { get; init; }

    public required string Content { get; init; }

    public Guid? StoryBeatId { get; init; }

    public SessionNoteStoryBeatResponse? StoryBeat { get; init; }

    public IReadOnlyList<SessionNoteStoryBeatReferenceResponse> StoryBeatReferences { get; init; } = [];

    public IReadOnlyList<SessionNoteChoiceResponse> Choices { get; init; } = [];

    public IReadOnlyList<SessionNoteMechanicsChangeResponse> MechanicsChanges { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SessionNoteStoryBeatResponse
{
    public Guid StoryBeatId { get; init; }

    public Guid StoryBlockId { get; init; }

    public int OrderIndex { get; init; }

    public int SecondaryOrderIndex { get; init; }

    public required string Title { get; init; }

    public StoryBeatType StoryBeatType { get; init; }
}

public sealed class SessionNoteStoryBeatReferenceResponse
{
    public int Id { get; init; }

    public int SessionNoteId { get; init; }

    public Guid StoryBeatId { get; init; }

    public SessionNoteStoryBeatReferenceType ReferenceType { get; init; }

    public Guid? ReferenceId { get; init; }

    public SessionNoteStoryBeatReferenceOutcome ReferenceOutcome { get; init; }

    public string? NpcTag { get; init; }

    public required string ContentSnapshot { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
