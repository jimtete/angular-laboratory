namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinsByMapResponse
{
    public int MapId { get; init; }

    public IReadOnlyList<MapPinTargetTypeResponse> PinTypes { get; init; } = [];

    public IReadOnlyList<MapPinDetailsResponse> Pins { get; init; } = [];

    public IReadOnlyList<MapPinConnectionResponse> Connections { get; init; } = [];
}
