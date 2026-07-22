namespace LearningLab.Data.Models.Monsters;

public class MonsterProficiency
{
    public int Id { get; set; }
    public int MonsterId { get; set; }
    public Monster Monster { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int? Bonus { get; set; }
    public string? Notes { get; set; }
}
