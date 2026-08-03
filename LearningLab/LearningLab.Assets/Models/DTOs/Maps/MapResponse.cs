using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Assets.Models.DTOs.Maps;

public sealed class MapResponse
{
    public int Id { get; init; }

    public int? ParentMapId { get; init; }

    public int AssetId { get; init; }

    public string? AssetUrl { get; init; }

    public string? ContentType { get; init; }

    public long? FileSizeBytes { get; init; }

    public MapCategory Category { get; init; }

    public int ImageWidthPixels { get; init; }

    public int ImageHeightPixels { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
