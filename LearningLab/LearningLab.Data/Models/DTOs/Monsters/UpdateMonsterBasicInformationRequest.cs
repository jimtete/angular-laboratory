namespace LearningLab.Data.Models.DTOs.Monsters;

public class UpdateMonsterBasicInformationRequest
{
    public string? Name { get; set; }
    public string? Size { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public List<string>? Tags { get; set; }
    public List<MonsterAbilityRequest>? Abilities { get; set; }
    public List<MonsterProficiencyRequest>? Proficiencies { get; set; }
    public string? Notes { get; set; }
}
