using LearningLab.Assets.Models.DTOs.Files;
using LearningLab.Data.Models;

namespace LearningLab.Assets.Services;

public interface IFileService
{
    Task<ServiceResult<IReadOnlyList<LibraryFolderResponse>>> GetFoldersAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<FileResponse>>> GetFilesAsync(
        Guid userId,
        int? parentFolderId = null,
        bool allFolders = false,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<LibraryFolderResponse>> CreateFolderAsync(
        Guid userId,
        CreateLibraryFolderRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteFolderAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<FileResponse>> GetFileAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<FileResponse>> CreateFileAsync(
        Guid userId,
        CreateFileRequest request,
        byte[]? fileBytes,
        string? originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<FileResponse>> UpdateFileAsync(
        Guid userId,
        int fileId,
        UpdateFileRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteFileAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default);
}
