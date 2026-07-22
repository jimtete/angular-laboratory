namespace LearningLab.Data.Models.Monsters;

public class MonsterAbility
{
    public int Id { get; set; }
    public int MonsterId { get; set; }
    public Monster Monster { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int? Value { get; set; }
    public int? Modifier { get; set; }
    public string? Notes { get; set; }
}
