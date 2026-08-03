using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class UpdateMapPinRequest
{
    public decimal XCoordinate { get; init; }

    public decimal YCoordinate { get; init; }

    public string? Label { get; init; }

    public string? Description { get; init; }

    public MapPinTargetType TargetType { get; init; }

    public string? TargetId { get; init; }
}
