namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterSpellSlotRequest
{
    public int SpellLevel { get; set; }
    public int? MaximumSlots { get; set; }
    public int? RemainingSlots { get; set; }
}
