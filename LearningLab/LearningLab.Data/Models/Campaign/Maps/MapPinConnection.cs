namespace LearningLab.Data.Models.Campaign.Maps;

public class MapPinConnection
{
    public int Id { get; set; }

    public int MapId { get; set; }

    public Map Map { get; set; } = null!;

    public int MapPinAId { get; set; }

    public MapPin MapPinA { get; set; } = null!;

    public int MapPinBId { get; set; }

    public MapPin MapPinB { get; set; } = null!;

    public decimal? DistanceValue { get; set; }

    public MapPinConnectionDistanceUnit? DistanceUnit { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
