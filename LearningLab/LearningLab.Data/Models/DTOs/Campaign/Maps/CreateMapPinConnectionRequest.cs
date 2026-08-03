using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class CreateMapPinConnectionRequest
{
    public int MapPinAId { get; init; }

    public int MapPinBId { get; init; }

    public decimal? DistanceValue { get; init; }

    public MapPinConnectionDistanceUnit? DistanceUnit { get; init; }
}
