namespace LearningLab.Data.Models.DTOs.Character;

public sealed class CharacterSheetResponse
{
    public Guid UserId { get; init; }

    public string? PortraitUrl { get; init; }

    public string? Background { get; init; }

    public string? Information { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string CharacterClass { get; init; }

    public string? Nationality { get; init; }

    public int? Height { get; init; }

    public int? Weight { get; init; }

    public IReadOnlyList<CharacterActionDto> Actions { get; init; } = [];

    public IReadOnlyList<string> Traits { get; init; } = [];

    public IReadOnlyList<string> Equipment { get; init; } = [];

    public int LogicRating { get; init; }

    public int PsycheRating { get; init; }

    public int PhysicalRating { get; init; }

    public int MotoricsRating { get; init; }
}
