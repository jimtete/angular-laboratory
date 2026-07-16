using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Assets;
using LearningLab.Services.AssetService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/assets")]
public sealed class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AssetResponse>>>> FetchAssets(
        [FromQuery] int? parentAssetId,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetAssetsAsync(
            parentAssetId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Assets fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidAsset => BadRequest(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Asset request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentNotFound => NotFound(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Parent asset was not found.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentMustBeFolder => BadRequest(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Parent asset must be a folder.",
                Data = null
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<AssetResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPost("folders")]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> CreateFolder(
        CreateAssetFolderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateFolderAsync(
            request,
            cancellationToken);

        return MapAssetMutationResponse(
            result,
            "Asset folder created successfully.");
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> CreateItem(
        CreateItemAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateItemAsync(
            request,
            cancellationToken);

        return MapAssetMutationResponse(
            result,
            "Item asset created successfully.");
    }

    [HttpPut("items/{assetId:int}")]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> UpdateItem(
        int assetId,
        UpdateItemAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateItemAsync(
            assetId,
            request,
            cancellationToken);

        return MapAssetUpdateResponse(
            result,
            "Item asset updated successfully.");
    }

    private ActionResult<ApiResponse<AssetResponse>> MapAssetMutationResponse(
        ServiceResult<AssetResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidAsset => BadRequest(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Asset request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentNotFound => NotFound(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Parent asset was not found.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentMustBeFolder => BadRequest(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Parent asset must be a folder.",
                Data = null
            }),
            ApplicationStatusCode.AssetAlreadyExists => Conflict(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "An asset with this name already exists in the selected folder.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "One or more campaigns were not found.",
                Data = null
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<AssetResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<AssetResponse>> MapAssetUpdateResponse(
        ServiceResult<AssetResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidAsset => BadRequest(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Asset request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.AssetNotFound => NotFound(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Asset was not found.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentNotFound => NotFound(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Parent asset was not found.",
                Data = null
            }),
            ApplicationStatusCode.AssetParentMustBeFolder => BadRequest(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Parent asset must be a folder.",
                Data = null
            }),
            ApplicationStatusCode.AssetAlreadyExists => Conflict(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "An asset with this name already exists in the selected folder.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<AssetResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "One or more campaigns were not found.",
                Data = null
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<AssetResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }
}
