using LearningLab.Data.Models.Assets;

namespace LearningLab.Data.Models.DTOs.Assets;

public class UpdateItemAssetRequest
{
    public int? ParentAssetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ItemType ItemType { get; set; }
    public List<Guid>? CampaignIds { get; set; }
}
