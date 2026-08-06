using LearningLab.Assets.Models.DTOs.Files;
using LearningLab.Assets.Services;
using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Parsers;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize]
[Route("api/library")]
public sealed class LibraryController : ControllerBase
{
    private readonly IFileService _fileService;

    public LibraryController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet("files")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FileResponse>>>> FetchFiles(
        [FromQuery] int? parentFolderId,
        [FromQuery] bool allFolders,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<FileResponse>>();
        }

        var result = await _fileService.GetFilesAsync(
            userId.Value,
            allFolders ? null : parentFolderId,
            allFolders,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<FileResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library files fetched successfully.",
                Data = result.Data?.Select(WithPublicStoragePath).ToList()
            }),
            _ => MapFileFailure<IReadOnlyList<FileResponse>>(result.StatusCode)
        };
    }

    [HttpGet("folders")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LibraryFolderResponse>>>> FetchFolders(
        [FromQuery] int? parentFolderId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<LibraryFolderResponse>>();
        }

        var result = await _fileService.GetFoldersAsync(
            userId.Value,
            parentFolderId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<LibraryFolderResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library folders fetched successfully.",
                Data = result.Data
            }),
            _ => MapFileFailure<IReadOnlyList<LibraryFolderResponse>>(result.StatusCode)
        };
    }

    [HttpPost("folders")]
    public async Task<ActionResult<ApiResponse<LibraryFolderResponse>>> CreateFolder(
        CreateLibraryFolderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<LibraryFolderResponse>();
        }

        var result = await _fileService.CreateFolderAsync(
            userId.Value,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<LibraryFolderResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Library folder created successfully.",
                Data = result.Data
            }),
            _ => MapFileFailure<LibraryFolderResponse>(result.StatusCode)
        };
    }

    [HttpDelete("folders/{folderId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFolder(
        int folderId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _fileService.DeleteFolderAsync(
            userId.Value,
            folderId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library folder deleted successfully.",
                Data = null
            }),
            _ => MapFileFailure<object>(result.StatusCode)
        };
    }

    [HttpGet("files/{fileId:int}")]
    public async Task<ActionResult<ApiResponse<FileResponse>>> FetchFile(
        int fileId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<FileResponse>();
        }

        var result = await _fileService.GetFileAsync(
            userId.Value,
            fileId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<FileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library file fetched successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicStoragePath(result.Data)
            }),
            _ => MapFileFailure<FileResponse>(result.StatusCode)
        };
    }

    [HttpPost("files")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<FileResponse>>> UploadFile(
        [FromForm] CreateFileRequest request,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<FileResponse>();
        }

        var fileBytes = await MediaParser.ReadUploadedFileBytesAsync(
            file,
            cancellationToken);

        var result = await _fileService.CreateFileAsync(
            userId.Value,
            request,
            fileBytes,
            file?.FileName,
            file?.ContentType,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<FileResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Library file uploaded successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicStoragePath(result.Data)
            }),
            _ => MapFileFailure<FileResponse>(result.StatusCode)
        };
    }

    [HttpPut("files/{fileId:int}")]
    public async Task<ActionResult<ApiResponse<FileResponse>>> UpdateFile(
        int fileId,
        UpdateFileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<FileResponse>();
        }

        var result = await _fileService.UpdateFileAsync(
            userId.Value,
            fileId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<FileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library file updated successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicStoragePath(result.Data)
            }),
            _ => MapFileFailure<FileResponse>(result.StatusCode)
        };
    }

    [HttpDelete("files/{fileId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFile(
        int fileId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _fileService.DeleteFileAsync(
            userId.Value,
            fileId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Library file deleted successfully.",
                Data = null
            }),
            _ => MapFileFailure<object>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<T>> MapFileFailure<T>(
        ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidFile => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "File request is invalid.",
                Data = default
            }),
            ApplicationStatusCode.FileRequired => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "File is required.",
                Data = default
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = default
            }),
            ApplicationStatusCode.FileNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "File was not found.",
                Data = default
            }),
            ApplicationStatusCode.InvalidLibraryFolder => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Library folder request is invalid.",
                Data = default
            }),
            ApplicationStatusCode.LibraryFolderNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Library folder was not found.",
                Data = default
            }),
            ApplicationStatusCode.LibraryFolderAlreadyExists => Conflict(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "A folder with this name already exists in the selected folder.",
                Data = default
            }),
            ApplicationStatusCode.FileTooLarge => StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge,
                    Message = "File must be 50 MB or smaller.",
                    Data = default
                }),
            ApplicationStatusCode.UnsupportedFileFormat => StatusCode(
                StatusCodes.Status415UnsupportedMediaType,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status415UnsupportedMediaType,
                    Message = "File type is not supported.",
                    Data = default
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = default
                })
        };
    }

    private FileResponse WithPublicStoragePath(FileResponse file)
    {
        return new FileResponse
        {
            Id = file.Id,
            UploadedByUserId = file.UploadedByUserId,
            ParentFolderId = file.ParentFolderId,
            DisplayName = file.DisplayName,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            StoragePath = Request.ToPublicStaticAssetUrl(file.StoragePath) ?? file.StoragePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.FileSizeBytes,
            DurationMilliseconds = file.DurationMilliseconds,
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
    }

    private UnauthorizedObjectResult InvalidUserClaimResponse<T>()
    {
        return Unauthorized(new ApiResponse<T>
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            Message = "The access token does not contain a valid user identifier.",
            Data = default
        });
    }
}
