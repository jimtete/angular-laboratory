using LearningLab.Data.Models.Assets;

namespace LearningLab.Assets.Repositories.FileRepository;

public interface IFileRepository
{
    Task<IReadOnlyList<MusicFile>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MusicFile>> ListByUserIdAndParentFolderIdAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MusicFile>> ListMutableByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibraryFolder>> ListFoldersByUserIdAndParentFolderIdAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibraryFolder>> ListMutableFoldersByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<LibraryFolder?> GetFolderByUserIdAndFolderIdAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default);

    Task<LibraryFolder?> GetMutableFolderByUserIdAndFolderIdAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default);

    Task<bool> FolderExistsByUserIdAndParentFolderIdAndNameAsync(
        Guid userId,
        int? parentFolderId,
        string name,
        CancellationToken cancellationToken = default);

    Task<MusicFile?> GetByUserIdAndFileIdAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default);

    Task<MusicFile?> GetMutableByUserIdAndFileIdAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        MusicFile file,
        CancellationToken cancellationToken = default);

    Task AddFolderAsync(
        LibraryFolder folder,
        CancellationToken cancellationToken = default);

    void Remove(MusicFile file);

    void RemoveRange(IReadOnlyCollection<MusicFile> files);

    void RemoveFolderRange(IReadOnlyCollection<LibraryFolder> folders);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
