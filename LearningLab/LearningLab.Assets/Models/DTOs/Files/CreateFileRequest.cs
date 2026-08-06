namespace LearningLab.Assets.Models.DTOs.Files;

public sealed class CreateFileRequest
{
    public int? ParentFolderId { get; init; }

    public string? DisplayName { get; init; }

    public int? DurationMilliseconds { get; init; }
}
