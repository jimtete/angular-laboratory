namespace LearningLab.Data.Models.Campaign.Maps;

public class MapPin
{
    public int Id { get; set; }

    public int MapId { get; set; }

    public Map Map { get; set; } = null!;

    public decimal XCoordinate { get; set; }

    public decimal YCoordinate { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public MapPinTargetType TargetType { get; set; }

    public string? TargetId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<MapPinConnection> ConnectionsAsA { get; set; } = [];

    public List<MapPinConnection> ConnectionsAsB { get; set; } = [];
}
