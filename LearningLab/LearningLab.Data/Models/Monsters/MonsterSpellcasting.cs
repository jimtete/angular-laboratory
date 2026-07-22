namespace LearningLab.Data.Models.Monsters;

public class MonsterSpellcasting
{
    public int MonsterId { get; set; }
    public Monster Monster { get; set; } = null!;
    public string? SpellcastingAbility { get; set; }
    public int? SpellSaveDC { get; set; }
    public int? SpellAttackBonus { get; set; }
    public string? Notes { get; set; }
    public List<MonsterSpellSlot> SpellSlots { get; set; } = [];
}
