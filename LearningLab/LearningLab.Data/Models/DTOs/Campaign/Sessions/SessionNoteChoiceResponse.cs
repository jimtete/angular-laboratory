namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteChoiceResponse
{
    public int Id { get; init; }

    public int SessionNoteId { get; init; }

    public int Order { get; init; }

    public required string ChoiceText { get; init; }

    public bool IsChosen { get; init; }
}
