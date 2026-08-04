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
[Route("api/campaigns/{campaignId:guid}/stores/{storeId:int}/lock-state")]
public sealed class CampaignStoreLockStateController : ControllerBase
{
    private readonly ICampaignStoreService _campaignStoreService;

    public CampaignStoreLockStateController(ICampaignStoreService campaignStoreService)
    {
        _campaignStoreService = campaignStoreService;
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> UpdateCampaignStoreLockState(
        Guid campaignId,
        int storeId,
        UpdateStoreLockStateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoreResponse>();
        }

        var result = await _campaignStoreService.UpdateCampaignStoreLockStateAsync(
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
                Message = "Campaign store lock state updated successfully.",
                Data = result.Data
            }),
            _ => StoreFailure<StoreResponse>(result.StatusCode)
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
                Message = "Campaign store lock state request is invalid.",
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
