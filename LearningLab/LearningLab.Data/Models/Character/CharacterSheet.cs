using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.Character;

public class CharacterSheet
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    
    public string? PortraitUrl { get; set; }
    
    public string? Background { get; set; }
    
    public string? Information { get; set; }
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string CharacterClass { get; set; } = string.Empty;

    public string? Nationality { get; set; }

    public int? Height { get; set; }

    public int? Weight { get; set; }

    public List<Action> Actions { get; set; } = [];

    public List<string> Traits { get; set; } = [];

    public List<string> Equipment { get; set; } = [];

    [Range(0, 15)]
    public int LogicRating { get; set; } = 0;

    [Range(0, 15)]
    public int PsycheRating { get; set; } = 0;

    [Range(0, 15)]
    public int PhysicalRating { get; set; } = 0;

    [Range(0, 15)]
    public int MotoricsRating { get; set; } = 0;
}
