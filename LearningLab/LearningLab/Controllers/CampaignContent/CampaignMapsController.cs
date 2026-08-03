using LearningLab.Assets.Models.DTOs.Maps;
using LearningLab.Assets.Services;
using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Parsers;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
[Route("api/campaigns/{campaignId:guid}/maps")]
public sealed class CampaignMapsController : ControllerBase
{
    private readonly IMapService _mapService;

    public CampaignMapsController(IMapService mapService)
    {
        _mapService = mapService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MapResponse>>>> FetchCampaignMaps(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<MapResponse>>();
        }

        var result = await _mapService.GetCampaignMapsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MapResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign maps fetched successfully.",
                Data = result.Data?.Select(WithPublicAssetUrl).ToList()
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<MapResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<MapResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<MapResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPost]
    [Authorize(Roles = AccessRoleNames.Master)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<MapResponse>>> UploadCampaignMap(
        Guid campaignId,
        [FromForm] CreateCampaignMapRequest request,
        IFormFile? mapFile,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapResponse>();
        }

        var mapFileBytes = await MediaParser.ReadMapFileBytesAsync(
            mapFile,
            cancellationToken);

        var result = await _mapService.CreateCampaignMapAsync(
            userId.Value,
            campaignId,
            request,
            mapFileBytes,
            mapFile?.ContentType,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign map uploaded successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrl(result.Data)
            }),
            ApplicationStatusCode.InvalidMap => BadRequest(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.MapFileRequired => BadRequest(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map file is required.",
                Data = null
            }),
            ApplicationStatusCode.InvalidMapParentHierarchy => BadRequest(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map parent hierarchy is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.MapParentNotFound => NotFound(new ApiResponse<MapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Parent map was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<MapResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can upload campaign maps.",
                    Data = null
                }),
            ApplicationStatusCode.MapFileTooLarge => StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new ApiResponse<MapResponse>
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge,
                    Message = "Map file must be 20 MB or smaller.",
                    Data = null
                }),
            ApplicationStatusCode.UnsupportedMapFileFormat => StatusCode(
                StatusCodes.Status415UnsupportedMediaType,
                new ApiResponse<MapResponse>
                {
                    StatusCode = StatusCodes.Status415UnsupportedMediaType,
                    Message = "Map file must be a JPEG, PNG, or WebP image.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<MapResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private MapResponse WithPublicAssetUrl(MapResponse map)
    {
        return new MapResponse
        {
            Id = map.Id,
            ParentMapId = map.ParentMapId,
            AssetId = map.AssetId,
            AssetUrl = Request.ToPublicStaticAssetUrl(map.AssetUrl),
            ContentType = map.ContentType,
            FileSizeBytes = map.FileSizeBytes,
            Category = map.Category,
            ImageWidthPixels = map.ImageWidthPixels,
            ImageHeightPixels = map.ImageHeightPixels,
            Name = map.Name,
            Description = map.Description,
            CreatedAt = map.CreatedAt,
            UpdatedAt = map.UpdatedAt
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
