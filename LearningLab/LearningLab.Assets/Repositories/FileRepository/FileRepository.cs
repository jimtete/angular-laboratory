using LearningLab.Data;
using LearningLab.Data.Models.Assets;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Assets.Repositories.FileRepository;

public sealed class FileRepository : IFileRepository
{
    private readonly LearningLabContext _context;

    public FileRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MusicFile>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MusicFiles
            .AsNoTracking()
            .Where(file => file.UploadedByUserId == userId)
            .OrderBy(file => file.DisplayName)
            .ThenBy(file => file.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MusicFile>> ListByUserIdAndParentFolderIdAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MusicFiles
            .AsNoTracking()
            .Where(file => file.UploadedByUserId == userId
                && file.ParentFolderId == parentFolderId)
            .OrderBy(file => file.DisplayName)
            .ThenBy(file => file.OriginalFileName)
            .ThenBy(file => file.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MusicFile>> ListMutableByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MusicFiles
            .Where(file => file.UploadedByUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LibraryFolder>> ListFoldersByUserIdAndParentFolderIdAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LibraryFolders
            .AsNoTracking()
            .Where(folder => folder.CreatedByUserId == userId
                && folder.ParentFolderId == parentFolderId)
            .OrderBy(folder => folder.Name)
            .ThenBy(folder => folder.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LibraryFolder>> ListMutableFoldersByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LibraryFolders
            .Where(folder => folder.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public Task<LibraryFolder?> GetFolderByUserIdAndFolderIdAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default)
    {
        return _context.LibraryFolders
            .AsNoTracking()
            .SingleOrDefaultAsync(
                folder => folder.CreatedByUserId == userId
                    && folder.Id == folderId,
                cancellationToken);
    }

    public Task<LibraryFolder?> GetMutableFolderByUserIdAndFolderIdAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default)
    {
        return _context.LibraryFolders
            .SingleOrDefaultAsync(
                folder => folder.CreatedByUserId == userId
                    && folder.Id == folderId,
                cancellationToken);
    }

    public Task<bool> FolderExistsByUserIdAndParentFolderIdAndNameAsync(
        Guid userId,
        int? parentFolderId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _context.LibraryFolders
            .AnyAsync(
                folder => folder.CreatedByUserId == userId
                    && folder.ParentFolderId == parentFolderId
                    && folder.Name == name,
                cancellationToken);
    }

    public Task<MusicFile?> GetByUserIdAndFileIdAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default)
    {
        return _context.MusicFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                file => file.UploadedByUserId == userId
                    && file.Id == fileId,
                cancellationToken);
    }

    public Task<MusicFile?> GetMutableByUserIdAndFileIdAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default)
    {
        return _context.MusicFiles
            .SingleOrDefaultAsync(
                file => file.UploadedByUserId == userId
                    && file.Id == fileId,
                cancellationToken);
    }

    public async Task AddAsync(
        MusicFile file,
        CancellationToken cancellationToken = default)
    {
        await _context.MusicFiles.AddAsync(file, cancellationToken);
    }

    public async Task AddFolderAsync(
        LibraryFolder folder,
        CancellationToken cancellationToken = default)
    {
        await _context.LibraryFolders.AddAsync(folder, cancellationToken);
    }

    public void Remove(MusicFile file)
    {
        _context.MusicFiles.Remove(file);
    }

    public void RemoveRange(IReadOnlyCollection<MusicFile> files)
    {
        _context.MusicFiles.RemoveRange(files);
    }

    public void RemoveFolderRange(IReadOnlyCollection<LibraryFolder> folders)
    {
        _context.LibraryFolders.RemoveRange(folders);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
