using LearningLab.Data.Models.Assets;

namespace LearningLab.Assets.Models.DTOs;

public class UpdateItemAssetRequest
{
    public int? ParentAssetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ItemType ItemType { get; set; }
    public List<Guid>? CampaignIds { get; set; }
}
