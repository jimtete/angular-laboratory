namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteMechanicsChangeResponse
{
    public int Id { get; init; }

    public int SessionNoteId { get; init; }

    public int Order { get; init; }

    public Guid PlayerId { get; init; }

    public string? ChangeText { get; init; }
}
