using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Character;

public sealed class UpdateCharacterSheetRequest
{
    public string? PortraitUrl { get; init; }

    public string? Background { get; init; }

    public string? Information { get; init; }

    [Required]
    public required string FirstName { get; init; }

    [Required]
    public required string LastName { get; init; }

    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public required string CharacterClass { get; init; }

    public string? Nationality { get; init; }

    public int? Height { get; init; }

    public int? Weight { get; init; }

    public List<CharacterActionDto> Actions { get; init; } = [];

    public List<string> Traits { get; init; } = [];

    public List<string> Equipment { get; init; } = [];

    [Range(0, 15)]
    public int LogicRating { get; init; }

    [Range(0, 15)]
    public int PsycheRating { get; init; }

    [Range(0, 15)]
    public int PhysicalRating { get; init; }

    [Range(0, 15)]
    public int MotoricsRating { get; init; }
}
