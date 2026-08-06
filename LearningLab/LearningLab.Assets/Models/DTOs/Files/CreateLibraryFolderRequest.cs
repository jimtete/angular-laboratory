namespace LearningLab.Assets.Models.DTOs.Files;

public sealed class CreateLibraryFolderRequest
{
    public int? ParentFolderId { get; init; }

    public string? Name { get; init; }
}
