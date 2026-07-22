namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterAbilityResponse
{
    public string Name { get; set; } = string.Empty;
    public int? Value { get; set; }
    public int? Modifier { get; set; }
    public string? Notes { get; set; }
}
