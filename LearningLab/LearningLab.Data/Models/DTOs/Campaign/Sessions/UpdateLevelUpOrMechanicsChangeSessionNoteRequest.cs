namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class UpdateLevelUpOrMechanicsChangeSessionNoteRequest
{
    public string? Content { get; init; }

    public IReadOnlyList<SessionNoteMechanicsChangeRequest> MechanicsChanges { get; init; } = [];
}
