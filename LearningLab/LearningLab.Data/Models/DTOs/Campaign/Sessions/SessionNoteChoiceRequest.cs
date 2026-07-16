namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class SessionNoteChoiceRequest
{
    public string? ChoiceText { get; init; }

    public bool IsChosen { get; init; }
}
