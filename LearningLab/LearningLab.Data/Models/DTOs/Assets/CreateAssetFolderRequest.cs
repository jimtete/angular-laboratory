namespace LearningLab.Data.Models.DTOs.Assets;

public class CreateAssetFolderRequest
{
    public int? ParentAssetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
