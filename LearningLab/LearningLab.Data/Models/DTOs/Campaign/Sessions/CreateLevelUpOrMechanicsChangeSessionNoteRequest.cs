namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CreateLevelUpOrMechanicsChangeSessionNoteRequest
{
    public string? Content { get; init; }

    public IReadOnlyList<SessionNoteMechanicsChangeRequest> MechanicsChanges { get; init; } = [];
}
