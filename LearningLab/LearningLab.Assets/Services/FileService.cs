using LearningLab.Assets.Configuration;
using LearningLab.Assets.Models.DTOs.Files;
using LearningLab.Assets.Repositories.FileRepository;
using LearningLab.Data.Models;
using LearningLab.Data.Models.Assets;
using LearningLab.Data.Repositories.UserRepository;
using Microsoft.Extensions.Options;

namespace LearningLab.Assets.Services;

public sealed class FileService : IFileService
{
    private const int MaximumDisplayNameLength = 256;
    private const int MaximumFolderNameLength = 256;

    private static readonly IReadOnlyDictionary<string, string> SupportedContentTypes = new Dictionary<string, string>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["audio/mpeg"] = ".mp3",
        ["audio/mp3"] = ".mp3",
        ["audio/wav"] = ".wav",
        ["audio/x-wav"] = ".wav",
        ["audio/ogg"] = ".ogg",
        ["audio/webm"] = ".webm",
        ["audio/aac"] = ".aac",
        ["audio/flac"] = ".flac",
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
        ["application/pdf"] = ".pdf",
        ["text/plain"] = ".txt",
        ["application/json"] = ".json",
        ["application/zip"] = ".zip"
    };

    private readonly IFileRepository _fileRepository;
    private readonly FileAssetStorageOptions _fileAssetStorageOptions;
    private readonly IUserRepository _userRepository;

    public FileService(
        IFileRepository fileRepository,
        IOptions<FileAssetStorageOptions> fileAssetStorageOptions,
        IUserRepository userRepository)
    {
        _fileRepository = fileRepository;
        _fileAssetStorageOptions = fileAssetStorageOptions.Value;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<LibraryFolderResponse>>> GetFoldersAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<LibraryFolderResponse>>(validationStatusCode.Value);
        }

        var parentValidationStatusCode = await ValidateParentFolderAsync(
            userId,
            parentFolderId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<LibraryFolderResponse>>(parentValidationStatusCode.Value);
        }

        var folders = await _fileRepository.ListFoldersByUserIdAndParentFolderIdAsync(
            userId,
            parentFolderId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<LibraryFolderResponse>>(
            ApplicationStatusCode.Success,
            folders.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<FileResponse>>> GetFilesAsync(
        Guid userId,
        int? parentFolderId = null,
        bool allFolders = false,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<FileResponse>>(validationStatusCode.Value);
        }

        if (!allFolders)
        {
            var parentValidationStatusCode = await ValidateParentFolderAsync(
                userId,
                parentFolderId,
                cancellationToken);

            if (parentValidationStatusCode is not null)
            {
                return new ServiceResult<IReadOnlyList<FileResponse>>(parentValidationStatusCode.Value);
            }
        }

        var files = allFolders
            ? await _fileRepository.ListByUserIdAsync(
                userId,
                cancellationToken)
            : await _fileRepository.ListByUserIdAndParentFolderIdAsync(
                userId,
                parentFolderId,
                cancellationToken);

        return new ServiceResult<IReadOnlyList<FileResponse>>(
            ApplicationStatusCode.Success,
            files.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<LibraryFolderResponse>> CreateFolderAsync(
        Guid userId,
        CreateLibraryFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request?.Name?.Trim();

        if (!IsValidFolderName(name))
        {
            return new ServiceResult<LibraryFolderResponse>(ApplicationStatusCode.InvalidLibraryFolder);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<LibraryFolderResponse>(validationStatusCode.Value);
        }

        var parentValidationStatusCode = await ValidateParentFolderAsync(
            userId,
            request?.ParentFolderId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<LibraryFolderResponse>(parentValidationStatusCode.Value);
        }

        var alreadyExists = await _fileRepository.FolderExistsByUserIdAndParentFolderIdAndNameAsync(
            userId,
            request?.ParentFolderId,
            name!,
            cancellationToken);

        if (alreadyExists)
        {
            return new ServiceResult<LibraryFolderResponse>(ApplicationStatusCode.LibraryFolderAlreadyExists);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var folder = new LibraryFolder
        {
            CreatedByUserId = userId,
            ParentFolderId = request?.ParentFolderId,
            Name = name!,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        await _fileRepository.AddFolderAsync(folder, cancellationToken);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<LibraryFolderResponse>(
            ApplicationStatusCode.Success,
            ToResponse(folder));
    }

    public async Task<ServiceResult<object>> DeleteFolderAsync(
        Guid userId,
        int folderId,
        CancellationToken cancellationToken = default)
    {
        if (folderId < 1)
        {
            return new ServiceResult<object>(ApplicationStatusCode.InvalidLibraryFolder);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var folder = await _fileRepository.GetMutableFolderByUserIdAndFolderIdAsync(
            userId,
            folderId,
            cancellationToken);

        if (folder is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.LibraryFolderNotFound);
        }

        var allFolders = await _fileRepository.ListMutableFoldersByUserIdAsync(
            userId,
            cancellationToken);
        var allFiles = await _fileRepository.ListMutableByUserIdAsync(
            userId,
            cancellationToken);
        var folderIdsToDelete = GetDescendantFolderIds(
            allFolders,
            folderId);
        var filesToDelete = allFiles
            .Where(file => file.ParentFolderId is not null
                && folderIdsToDelete.Contains(file.ParentFolderId.Value))
            .ToList();
        var foldersToDelete = allFolders
            .Where(candidate => folderIdsToDelete.Contains(candidate.Id))
            .OrderByDescending(candidate => candidate.ParentFolderId.HasValue)
            .ToList();

        foreach (var file in filesToDelete)
        {
            DeleteStoredFile(file.StoragePath);
        }

        _fileRepository.RemoveRange(filesToDelete);
        _fileRepository.RemoveFolderRange(foldersToDelete);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<FileResponse>> GetFileAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default)
    {
        if (fileId < 1)
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.InvalidFile);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<FileResponse>(validationStatusCode.Value);
        }

        var file = await _fileRepository.GetByUserIdAndFileIdAsync(
            userId,
            fileId,
            cancellationToken);

        return file is null
            ? new ServiceResult<FileResponse>(ApplicationStatusCode.FileNotFound)
            : new ServiceResult<FileResponse>(
                ApplicationStatusCode.Success,
                ToResponse(file));
    }

    public async Task<ServiceResult<FileResponse>> CreateFileAsync(
        Guid userId,
        CreateFileRequest request,
        byte[]? fileBytes,
        string? originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var displayName = request?.DisplayName?.Trim();
        var normalizedOriginalFileName = NormalizeOriginalFileName(originalFileName);

        if (!IsValidFileMetadata(displayName, request?.DurationMilliseconds))
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.InvalidFile);
        }

        if (fileBytes is null || fileBytes.Length == 0)
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.FileRequired);
        }

        if (fileBytes.LongLength > _fileAssetStorageOptions.MaxFileSizeBytes)
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.FileTooLarge);
        }

        if (!IsSupportedContentType(contentType))
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.UnsupportedFileFormat);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<FileResponse>(validationStatusCode.Value);
        }

        var parentValidationStatusCode = await ValidateParentFolderAsync(
            userId,
            request?.ParentFolderId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<FileResponse>(parentValidationStatusCode.Value);
        }

        var timestamp = DateTimeOffset.UtcNow;
        var (storagePath, storedFileName) = await StoreFileAsync(
            userId,
            fileBytes,
            contentType,
            cancellationToken);

        var file = new MusicFile
        {
            UploadedByUserId = userId,
            ParentFolderId = request?.ParentFolderId,
            DisplayName = displayName!,
            OriginalFileName = normalizedOriginalFileName,
            StoredFileName = storedFileName,
            StoragePath = storagePath,
            ContentType = NormalizeContentType(contentType),
            FileSizeBytes = fileBytes.LongLength,
            DurationMilliseconds = request?.DurationMilliseconds,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        await _fileRepository.AddAsync(file, cancellationToken);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<FileResponse>(
            ApplicationStatusCode.Success,
            ToResponse(file));
    }

    public async Task<ServiceResult<FileResponse>> UpdateFileAsync(
        Guid userId,
        int fileId,
        UpdateFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var displayName = request?.DisplayName?.Trim();

        if (fileId < 1 || !IsValidFileMetadata(displayName, request?.DurationMilliseconds))
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.InvalidFile);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<FileResponse>(validationStatusCode.Value);
        }

        var parentValidationStatusCode = await ValidateParentFolderAsync(
            userId,
            request?.ParentFolderId,
            cancellationToken);

        if (parentValidationStatusCode is not null)
        {
            return new ServiceResult<FileResponse>(parentValidationStatusCode.Value);
        }

        var file = await _fileRepository.GetMutableByUserIdAndFileIdAsync(
            userId,
            fileId,
            cancellationToken);

        if (file is null)
        {
            return new ServiceResult<FileResponse>(ApplicationStatusCode.FileNotFound);
        }

        file.ParentFolderId = request?.ParentFolderId;
        file.DisplayName = displayName!;
        file.DurationMilliseconds = request?.DurationMilliseconds;
        file.UpdatedAt = DateTimeOffset.UtcNow;

        await _fileRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<FileResponse>(
            ApplicationStatusCode.Success,
            ToResponse(file));
    }

    public async Task<ServiceResult<object>> DeleteFileAsync(
        Guid userId,
        int fileId,
        CancellationToken cancellationToken = default)
    {
        if (fileId < 1)
        {
            return new ServiceResult<object>(ApplicationStatusCode.InvalidFile);
        }

        var validationStatusCode = await ValidateUserAsync(
            userId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var file = await _fileRepository.GetMutableByUserIdAndFileIdAsync(
            userId,
            fileId,
            cancellationToken);

        if (file is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.FileNotFound);
        }

        DeleteStoredFile(file.StoragePath);
        _fileRepository.Remove(file);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    private async Task<ApplicationStatusCode?> ValidateUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return user is null
            ? ApplicationStatusCode.UserNotFound
            : null;
    }

    private async Task<ApplicationStatusCode?> ValidateParentFolderAsync(
        Guid userId,
        int? parentFolderId,
        CancellationToken cancellationToken)
    {
        if (parentFolderId is null)
        {
            return null;
        }

        if (parentFolderId < 1)
        {
            return ApplicationStatusCode.InvalidLibraryFolder;
        }

        var folder = await _fileRepository.GetFolderByUserIdAndFolderIdAsync(
            userId,
            parentFolderId.Value,
            cancellationToken);

        return folder is null
            ? ApplicationStatusCode.LibraryFolderNotFound
            : null;
    }

    private async Task<(string StoragePath, string StoredFileName)> StoreFileAsync(
        Guid userId,
        byte[] fileBytes,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var userFolderName = userId.ToString("D");
        var fileExtension = SupportedContentTypes[contentType ?? string.Empty];
        var storedFileName = $"file_{Guid.NewGuid():N}{fileExtension}";
        var assetDirectory = Path.Combine(
            _fileAssetStorageOptions.RootPath,
            "users",
            userFolderName,
            "files");
        var filePath = Path.Combine(assetDirectory, storedFileName);

        Directory.CreateDirectory(assetDirectory);
        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);

        var requestPath = _fileAssetStorageOptions.RequestPath.TrimEnd('/');
        return ($"{requestPath}/users/{userFolderName}/files/{storedFileName}", storedFileName);
    }

    private void DeleteStoredFile(string storagePath)
    {
        var filePath = ResolveStoragePath(storagePath);

        if (filePath is not null && File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private string? ResolveStoragePath(string storagePath)
    {
        var requestPath = _fileAssetStorageOptions.RequestPath.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(storagePath)
            || !storagePath.StartsWith(requestPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relativePath = storagePath[(requestPath.Length + 1)..]
            .Replace('/', Path.DirectorySeparatorChar);
        var rootPath = Path.GetFullPath(_fileAssetStorageOptions.RootPath);
        var filePath = Path.GetFullPath(Path.Combine(rootPath, relativePath));

        return filePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)
            ? filePath
            : null;
    }

    private static bool IsValidFileMetadata(
        string? displayName,
        int? durationMilliseconds)
    {
        return !string.IsNullOrWhiteSpace(displayName)
            && displayName.Length <= MaximumDisplayNameLength
            && (durationMilliseconds is null || durationMilliseconds >= 0);
    }

    private static bool IsValidFolderName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name)
            && name.Length <= MaximumFolderNameLength
            && !name.Contains('/')
            && !name.Contains('\\');
    }

    private static HashSet<int> GetDescendantFolderIds(
        IReadOnlyCollection<LibraryFolder> folders,
        int rootFolderId)
    {
        var folderIds = new HashSet<int> { rootFolderId };
        var didAddFolder = true;

        while (didAddFolder)
        {
            didAddFolder = false;

            foreach (var folder in folders)
            {
                if (folder.ParentFolderId is not null
                    && folderIds.Contains(folder.ParentFolderId.Value)
                    && folderIds.Add(folder.Id))
                {
                    didAddFolder = true;
                }
            }
        }

        return folderIds;
    }

    private static bool IsSupportedContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
            && SupportedContentTypes.ContainsKey(contentType);
    }

    private static string NormalizeContentType(string? contentType)
    {
        if (string.Equals(contentType, "audio/mp3", StringComparison.OrdinalIgnoreCase))
        {
            return "audio/mpeg";
        }

        if (string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase))
        {
            return "image/jpeg";
        }

        return contentType ?? string.Empty;
    }

    private static string NormalizeOriginalFileName(string? originalFileName)
    {
        return string.IsNullOrWhiteSpace(originalFileName)
            ? string.Empty
            : Path.GetFileName(originalFileName.Trim());
    }

    private static FileResponse ToResponse(MusicFile file)
    {
        return new FileResponse
        {
            Id = file.Id,
            UploadedByUserId = file.UploadedByUserId,
            ParentFolderId = file.ParentFolderId,
            DisplayName = file.DisplayName,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            StoragePath = file.StoragePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.FileSizeBytes,
            DurationMilliseconds = file.DurationMilliseconds,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
    }

    private static LibraryFolderResponse ToResponse(LibraryFolder folder)
    {
        return new LibraryFolderResponse
        {
            Id = folder.Id,
            CreatedByUserId = folder.CreatedByUserId,
            ParentFolderId = folder.ParentFolderId,
            Name = folder.Name,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }
}
