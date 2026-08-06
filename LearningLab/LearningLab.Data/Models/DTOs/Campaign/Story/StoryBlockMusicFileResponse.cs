namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBlockMusicFileResponse
{
    public Guid Id { get; init; }

    public Guid StoryBlockId { get; init; }

    public Guid? StoryBeatId { get; init; }

    public int MusicFileId { get; init; }

    public int OrderIndex { get; init; }

    public bool Loop { get; init; }

    public bool ContinueAcrossStoryBlocks { get; init; }

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
