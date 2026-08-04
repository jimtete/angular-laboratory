using LearningLab.Data.Models.Campaign.Maps;
using LearningLab.Data.Models.Campaign.Stores;

namespace LearningLab.Data.Models.DTOs.Campaign.Maps;

public sealed class MapPinTargetDataResponse
{
    public MapPinTargetType TargetType { get; init; }

    public string? TargetId { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public Guid? StoryBlockId { get; init; }

    public int? StoryBlockOrderIndex { get; init; }

    public int? MapId { get; init; }

    public int? ParentMapId { get; init; }

    public int? AssetId { get; init; }

    public string? AssetUrl { get; init; }

    public string? ContentType { get; init; }

    public MapCategory? MapCategory { get; init; }

    public int? ImageWidthPixels { get; init; }

    public int? ImageHeightPixels { get; init; }

    public int? StoreId { get; init; }

    public StoreType? StoreType { get; init; }

    public StoreLockState? StoreLockState { get; init; }

    public string? StoreLocation { get; init; }
}
