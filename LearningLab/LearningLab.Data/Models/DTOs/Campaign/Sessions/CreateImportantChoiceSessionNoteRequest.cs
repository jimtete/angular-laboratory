namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CreateImportantChoiceSessionNoteRequest
{
    public string? Content { get; init; }

    public IReadOnlyList<SessionNoteChoiceRequest> Choices { get; init; } = [];
}
