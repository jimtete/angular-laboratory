using LearningLab.Data.Models.Assets;

namespace LearningLab.Assets.Models.DTOs;

public class AssetResponse
{
    public int Id { get; set; }
    public int? ParentAssetId { get; set; }
    public AssetType AssetType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType? ItemType { get; set; }
    public List<Guid>? CampaignIds { get; set; }
    public string? AssetUrl { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
