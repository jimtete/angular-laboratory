namespace LearningLab.Assets.Models.DTOs.Files;

public sealed class FileResponse
{
    public int Id { get; init; }

    public Guid UploadedByUserId { get; init; }

    public int? ParentFolderId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string OriginalFileName { get; init; } = string.Empty;

    public string StoredFileName { get; init; } = string.Empty;

    public string StoragePath { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSizeBytes { get; init; }

    public int? DurationMilliseconds { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
