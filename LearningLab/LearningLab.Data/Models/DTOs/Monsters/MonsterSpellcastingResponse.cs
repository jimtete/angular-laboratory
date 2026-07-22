namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterSpellcastingResponse
{
    public string? SpellcastingAbility { get; set; }
    public int? SpellSaveDC { get; set; }
    public int? SpellAttackBonus { get; set; }
    public string? Notes { get; set; }
    public List<MonsterSpellSlotResponse> SpellSlots { get; set; } = [];
}
