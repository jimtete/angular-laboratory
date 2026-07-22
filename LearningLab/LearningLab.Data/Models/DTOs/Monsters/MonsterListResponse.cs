namespace LearningLab.Data.Models.DTOs.Monsters;

public class MonsterListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public List<string>? Tags { get; set; }
}
