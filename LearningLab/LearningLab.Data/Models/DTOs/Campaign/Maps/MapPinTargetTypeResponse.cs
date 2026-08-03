using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinTargetTypeResponse
{
    public int Id { get; init; }

    public MapPinTargetType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool RequiresTargetId { get; init; }
}
