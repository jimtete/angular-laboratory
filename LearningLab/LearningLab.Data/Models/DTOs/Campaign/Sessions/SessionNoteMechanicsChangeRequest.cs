namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteMechanicsChangeRequest
{
    public Guid PlayerId { get; init; }

    public string? ChangeText { get; init; }
}
