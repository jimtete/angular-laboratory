namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterProficiencyResponse
{
    public string Name { get; set; } = string.Empty;
    public int? Bonus { get; set; }
    public string? Notes { get; set; }
}
