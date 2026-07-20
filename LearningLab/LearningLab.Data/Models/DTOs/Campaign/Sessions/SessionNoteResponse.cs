using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteResponse
{
    public int Id { get; init; }

    public int SessionId { get; init; }

    public int Order { get; init; }

    public SessionNoteType Type { get; init; }

    public required string Content { get; init; }

    public IReadOnlyList<SessionNoteChoiceResponse> Choices { get; init; } = [];

    public IReadOnlyList<SessionNoteMechanicsChangeResponse> MechanicsChanges { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
