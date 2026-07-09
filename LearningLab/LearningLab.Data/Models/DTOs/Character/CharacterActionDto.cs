using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Character;

public sealed class CharacterActionDto
{
    public ActionType ActionType { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }
}
