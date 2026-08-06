using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Assets;

public class MusicFile
{
    public int Id { get; set; }

    public Guid UploadedByUserId { get; set; }

    public User UploadedByUser { get; set; } = null!;

    public int? ParentFolderId { get; set; }

    public LibraryFolder? ParentFolder { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int? DurationMilliseconds { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<StoryBlockMusicFile> StoryBlockLinks { get; set; } = [];
}
