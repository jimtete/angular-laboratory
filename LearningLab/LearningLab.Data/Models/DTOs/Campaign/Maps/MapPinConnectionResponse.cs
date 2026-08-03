using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinConnectionResponse
{
    public int Id { get; init; }

    public int MapId { get; init; }

    public int MapPinAId { get; init; }

    public int MapPinBId { get; init; }

    public decimal? DistanceValue { get; init; }

    public MapPinConnectionDistanceUnit? DistanceUnit { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
