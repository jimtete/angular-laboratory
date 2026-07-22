namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public List<string>? Tags { get; set; }
    public List<MonsterAbilityResponse> Abilities { get; set; } = [];
    public List<MonsterProficiencyResponse> Proficiencies { get; set; } = [];
    public MonsterSpellcastingResponse? Spellcasting { get; set; }
    public List<MonsterFeatureResponse> Features { get; set; } = [];
    public string? Notes { get; set; }
}
