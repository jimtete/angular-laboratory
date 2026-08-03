using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinConnectionDistanceUnitResponse
{
    public int Id { get; init; }

    public MapPinConnectionDistanceUnit Unit { get; init; }

    public string Name { get; init; } = string.Empty;
}
