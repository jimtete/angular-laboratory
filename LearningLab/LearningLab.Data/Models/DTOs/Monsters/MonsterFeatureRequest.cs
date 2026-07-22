using LearningLab.Data.Models.Monsters;

namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterFeatureRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MonsterFeatureCategory Category { get; set; }
    public string? UsageNote { get; set; }
    public int? ResourceCost { get; set; }
    public bool IsSpell { get; set; }
    public int? SpellLevel { get; set; }
    public string? CastingTime { get; set; }
    public string? Range { get; set; }
    public string? Duration { get; set; }
    public bool? Concentration { get; set; }
    public int SortOrder { get; set; }
}
