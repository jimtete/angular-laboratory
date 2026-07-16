namespace LearningLab.Data.Models.Assets;

public class Asset
{
    public int Id { get; set; }
    public int? ParentAssetId { get; set; }
    public Asset? ParentAsset { get; set; }
    public List<Asset> Children { get; set; } = [];
    public AssetType AssetType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType? ItemType { get; set; }
    public List<Guid>? CampaignIds { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
