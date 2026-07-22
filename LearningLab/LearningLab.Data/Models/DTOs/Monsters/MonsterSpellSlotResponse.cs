namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterSpellSlotResponse
{
    public int SpellLevel { get; set; }
    public int? MaximumSlots { get; set; }
    public int? RemainingSlots { get; set; }
}
