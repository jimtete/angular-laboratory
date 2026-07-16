namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class UpdateImportantChoiceSessionNoteRequest
{
    public string? Content { get; init; }

    public IReadOnlyList<SessionNoteChoiceRequest> Choices { get; init; } = [];
}
