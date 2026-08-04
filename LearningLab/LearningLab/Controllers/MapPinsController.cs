using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Maps;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Services.CampaignMapPinService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
[Route("api/map-pins")]
public sealed class MapPinsController : ControllerBase
{
    private readonly ICampaignMapPinService _campaignMapPinService;

    public MapPinsController(ICampaignMapPinService campaignMapPinService)
    {
        _campaignMapPinService = campaignMapPinService;
    }

    [HttpGet("types")]
    public ActionResult<ApiResponse<IReadOnlyList<MapPinTargetTypeResponse>>> FetchMapPinTypes()
    {
        var result = _campaignMapPinService.GetMapPinTargetTypes();

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MapPinTargetTypeResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pin target types fetched successfully.",
                Data = result.Data
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<MapPinTargetTypeResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpGet("connection-distance-units")]
    public ActionResult<ApiResponse<IReadOnlyList<MapPinConnectionDistanceUnitResponse>>> FetchConnectionDistanceUnits()
    {
        var result = _campaignMapPinService.GetMapPinConnectionDistanceUnits();

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MapPinConnectionDistanceUnitResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pin connection distance units fetched successfully.",
                Data = result.Data
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<MapPinConnectionDistanceUnitResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpGet("campaigns/{campaignId:guid}/maps/{mapId:int}")]
    public async Task<ActionResult<ApiResponse<MapPinsByMapResponse>>> FetchMapPinsByMap(
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapPinsByMapResponse>();
        }

        var result = await _campaignMapPinService.GetMapPinsByMapAsync(
            userId.Value,
            campaignId,
            mapId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MapPinsByMapResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pins fetched successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrls(result.Data)
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<MapPinsByMapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<MapPinsByMapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.MapNotFound => NotFound(new ApiResponse<MapPinsByMapResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map was not found.",
                Data = null
            }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<MapPinsByMapResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private MapPinsByMapResponse WithPublicAssetUrls(MapPinsByMapResponse response)
    {
        return new MapPinsByMapResponse
        {
            MapId = response.MapId,
            PinTypes = response.PinTypes,
            Connections = response.Connections,
            Pins = response.Pins
                .Select(pin => new MapPinDetailsResponse
                {
                    Id = pin.Id,
                    MapId = pin.MapId,
                    XCoordinate = pin.XCoordinate,
                    YCoordinate = pin.YCoordinate,
                    Label = pin.Label,
                    Description = pin.Description,
                    TargetType = pin.TargetType,
                    TargetId = pin.TargetId,
                    TargetData = WithPublicAssetUrl(pin.TargetData),
                    CreatedAt = pin.CreatedAt,
                    UpdatedAt = pin.UpdatedAt
                })
                .ToList()
        };
    }

    private MapPinTargetDataResponse? WithPublicAssetUrl(MapPinTargetDataResponse? targetData)
    {
        if (targetData is null)
        {
            return null;
        }

        return new MapPinTargetDataResponse
        {
            TargetType = targetData.TargetType,
            TargetId = targetData.TargetId,
            Name = targetData.Name,
            Description = targetData.Description,
            StoryBlockId = targetData.StoryBlockId,
            StoryBlockOrderIndex = targetData.StoryBlockOrderIndex,
            MapId = targetData.MapId,
            ParentMapId = targetData.ParentMapId,
            AssetId = targetData.AssetId,
            AssetUrl = Request.ToPublicStaticAssetUrl(targetData.AssetUrl),
            ContentType = targetData.ContentType,
            MapCategory = targetData.MapCategory,
            ImageWidthPixels = targetData.ImageWidthPixels,
            ImageHeightPixels = targetData.ImageHeightPixels,
            StoreId = targetData.StoreId,
            StoreType = targetData.StoreType,
            StoreLockState = targetData.StoreLockState,
            StoreLocation = targetData.StoreLocation
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
