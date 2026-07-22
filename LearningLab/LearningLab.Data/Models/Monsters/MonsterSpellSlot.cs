namespace LearningLab.Data.Models.Monsters;

public class MonsterSpellSlot
{
    public int Id { get; set; }
    public int MonsterSpellcastingId { get; set; }
    public MonsterSpellcasting Spellcasting { get; set; } = null!;
    public int SpellLevel { get; set; }
    public int? MaximumSlots { get; set; }
    public int? RemainingSlots { get; set; }
}
