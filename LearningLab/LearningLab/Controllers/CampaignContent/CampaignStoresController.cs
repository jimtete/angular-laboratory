using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Stores;
using LearningLab.Services.CampaignStoreService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/stores")]
public sealed class CampaignStoresController : ControllerBase
{
    private readonly ICampaignStoreService _campaignStoreService;

    public CampaignStoresController(ICampaignStoreService campaignStoreService)
    {
        _campaignStoreService = campaignStoreService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoreResponse>>>> FetchCampaignStores(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoreResponse>>();
        }

        var result = await _campaignStoreService.GetCampaignStoresAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoreResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign stores fetched successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<IReadOnlyList<StoreResponse>>(result.StatusCode)
        };
    }

    [HttpGet("{storeId:int}")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> FetchCampaignStore(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoreResponse>();
        }

        var result = await _campaignStoreService.GetCampaignStoreAsync(
            userId.Value,
            campaignId,
            storeId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoreResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign store fetched successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<StoreResponse>(result.StatusCode)
        };
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> CreateCampaignStore(
        Guid campaignId,
        CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoreResponse>();
        }

        var result = await _campaignStoreService.CreateCampaignStoreAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoreResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign store created successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<StoreResponse>(result.StatusCode)
        };
    }

    [HttpPut("{storeId:int}")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> UpdateCampaignStore(
        Guid campaignId,
        int storeId,
        UpdateStoreRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoreResponse>();
        }

        var result = await _campaignStoreService.UpdateCampaignStoreAsync(
            userId.Value,
            campaignId,
            storeId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoreResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign store updated successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<StoreResponse>(result.StatusCode)
        };
    }

    [HttpPut("{storeId:int}/item-purchases")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> UpdateCampaignStoreItemPurchases(
        Guid campaignId,
        int storeId,
        UpdateStoreItemPurchaseStateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoreResponse>();
        }

        var result = await _campaignStoreService.UpdateCampaignStoreItemPurchasesAsync(
            userId.Value,
            campaignId,
            storeId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoreResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign store purchases saved successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<StoreResponse>(result.StatusCode)
        };
    }

    [HttpDelete("{storeId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCampaignStore(
        Guid campaignId,
        int storeId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignStoreService.DeleteCampaignStoreAsync(
            userId.Value,
            campaignId,
            storeId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign store deleted successfully.",
                Data = null
            }),
            _ => StoreFailure<object>(result.StatusCode)
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

    private ActionResult<ApiResponse<T>> StoreFailure<T>(
        ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidStore => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign store request is invalid.",
                Data = default
            }),
            ApplicationStatusCode.InvalidCampaignSettings => BadRequest(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign store mechanics setting is invalid.",
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
            ApplicationStatusCode.StoreNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign store was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign stores.",
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
}
