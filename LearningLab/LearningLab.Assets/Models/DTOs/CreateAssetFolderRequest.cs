namespace LearningLab.Assets.Models.DTOs;

public class CreateAssetFolderRequest
{
    public int? ParentAssetId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
