namespace LearningLab.Data.Models.Assets;

public class LibraryFolder
{
    public int Id { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public int? ParentFolderId { get; set; }

    public LibraryFolder? ParentFolder { get; set; }

    public List<LibraryFolder> Children { get; set; } = [];

    public List<MusicFile> Files { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
