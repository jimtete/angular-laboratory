using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.Monsters;

public class Monster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public List<string>? Tags { get; set; }
    public List<MonsterAbility> Abilities { get; set; } = [];
    public List<MonsterProficiency> Proficiencies { get; set; } = [];
    public MonsterSpellcasting? Spellcasting { get; set; }
    public List<MonsterFeature> Features { get; set; } = [];
    public List<CampaignNpcParticipation> CampaignParticipations { get; set; } = [];
    public string? Notes { get; set; }
}
