using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinResponse
{
    public int Id { get; init; }

    public int MapId { get; init; }

    public decimal XCoordinate { get; init; }

    public decimal YCoordinate { get; init; }

    public string Label { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public MapPinTargetType TargetType { get; init; }

    public string? TargetId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
