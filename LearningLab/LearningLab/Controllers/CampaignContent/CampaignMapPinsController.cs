using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Maps;
using LearningLab.Services.CampaignMapPinService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
[Route("api/campaigns/{campaignId:guid}/maps/{mapId:int}/pins")]
public sealed class CampaignMapPinsController : ControllerBase
{
    private readonly ICampaignMapPinService _campaignMapPinService;

    public CampaignMapPinsController(ICampaignMapPinService campaignMapPinService)
    {
        _campaignMapPinService = campaignMapPinService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MapPinResponse>>>> FetchMapPins(
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<MapPinResponse>>();
        }

        var result = await _campaignMapPinService.GetMapPinsAsync(
            userId.Value,
            campaignId,
            mapId,
            cancellationToken);

        return MapPinListResponse(
            result,
            "Map pins fetched successfully.");
    }

    [HttpPost]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<MapPinResponse>>> CreateMapPin(
        Guid campaignId,
        int mapId,
        CreateMapPinRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapPinResponse>();
        }

        var result = await _campaignMapPinService.CreateMapPinAsync(
            userId.Value,
            campaignId,
            mapId,
            request,
            cancellationToken);

        return MapPinMutationResponse(
            result,
            "Map pin created successfully.",
            created: true);
    }

    [HttpGet("connections")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MapPinConnectionResponse>>>> FetchMapPinConnections(
        Guid campaignId,
        int mapId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<MapPinConnectionResponse>>();
        }

        var result = await _campaignMapPinService.GetMapPinConnectionsAsync(
            userId.Value,
            campaignId,
            mapId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MapPinConnectionResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pin connections fetched successfully.",
                Data = result.Data
            }),
            _ => MapPinConnectionFailure<IReadOnlyList<MapPinConnectionResponse>>(result.StatusCode)
        };
    }

    [HttpPost("connections")]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<MapPinConnectionResponse>>> CreateMapPinConnection(
        Guid campaignId,
        int mapId,
        CreateMapPinConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapPinConnectionResponse>();
        }

        var result = await _campaignMapPinService.CreateMapPinConnectionAsync(
            userId.Value,
            campaignId,
            mapId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<MapPinConnectionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Map pin connection created successfully.",
                Data = result.Data
            }),
            _ => MapPinConnectionFailure<MapPinConnectionResponse>(result.StatusCode)
        };
    }

    [HttpPut("connections/{connectionId:int}")]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<MapPinConnectionResponse>>> UpdateMapPinConnection(
        Guid campaignId,
        int mapId,
        int connectionId,
        UpdateMapPinConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapPinConnectionResponse>();
        }

        var result = await _campaignMapPinService.UpdateMapPinConnectionAsync(
            userId.Value,
            campaignId,
            mapId,
            connectionId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MapPinConnectionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pin connection updated successfully.",
                Data = result.Data
            }),
            _ => MapPinConnectionFailure<MapPinConnectionResponse>(result.StatusCode)
        };
    }

    [HttpPut("{pinId:int}")]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<MapPinResponse>>> UpdateMapPin(
        Guid campaignId,
        int mapId,
        int pinId,
        UpdateMapPinRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MapPinResponse>();
        }

        var result = await _campaignMapPinService.UpdateMapPinAsync(
            userId.Value,
            campaignId,
            mapId,
            pinId,
            request,
            cancellationToken);

        return MapPinMutationResponse(
            result,
            "Map pin updated successfully.",
            created: false);
    }

    [HttpDelete("{pinId:int}")]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMapPin(
        Guid campaignId,
        int mapId,
        int pinId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignMapPinService.DeleteMapPinAsync(
            userId.Value,
            campaignId,
            mapId,
            pinId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Map pin deleted successfully.",
                Data = null
            }),
            _ => MapPinFailure<object>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<MapPinResponse>>> MapPinListResponse(
        ServiceResult<IReadOnlyList<MapPinResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MapPinResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            _ => MapPinFailure<IReadOnlyList<MapPinResponse>>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<MapPinResponse>> MapPinMutationResponse(
        ServiceResult<MapPinResponse> result,
        string successMessage,
        bool created)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success when created => Created(string.Empty, new ApiResponse<MapPinResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.Success => Ok(new ApiResponse<MapPinResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            _ => MapPinFailure<MapPinResponse>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<T>> MapPinConnectionFailure<T>(
        ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidMapPinConnection => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map pin connection request is invalid.",
                Data = default
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = default
            }),
            ApplicationStatusCode.MapNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map was not found.",
                Data = default
            }),
            ApplicationStatusCode.MapPinNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "One or more map pins were not found.",
                Data = default
            }),
            ApplicationStatusCode.MapPinConnectionAlreadyExists => Conflict(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Map pin connection already exists.",
                Data = default
            }),
            ApplicationStatusCode.MapPinConnectionNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map pin connection was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage map pin connections.",
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

    private ActionResult<ApiResponse<T>> MapPinFailure<T>(
        ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidMapPin => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map pin request is invalid.",
                Data = default
            }),
            ApplicationStatusCode.InvalidMapPinTarget => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Map pin target is invalid.",
                Data = default
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = default
            }),
            ApplicationStatusCode.MapNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map was not found.",
                Data = default
            }),
            ApplicationStatusCode.MapPinNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map pin was not found.",
                Data = default
            }),
            ApplicationStatusCode.MapPinTargetNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Map pin target was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage map pins.",
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
