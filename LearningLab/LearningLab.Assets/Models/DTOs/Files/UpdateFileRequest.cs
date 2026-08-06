namespace LearningLab.Assets.Models.DTOs.Files;

public sealed class UpdateFileRequest
{
    public int? ParentFolderId { get; init; }

    public string? DisplayName { get; init; }

    public int? DurationMilliseconds { get; init; }
}
