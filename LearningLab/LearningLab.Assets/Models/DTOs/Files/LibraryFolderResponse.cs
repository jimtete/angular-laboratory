namespace LearningLab.Assets.Models.DTOs.Files;

public sealed class LibraryFolderResponse
{
    public int Id { get; init; }

    public Guid CreatedByUserId { get; init; }

    public int? ParentFolderId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
