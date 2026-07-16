using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Assets;
using LearningLab.Services.AssetService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/assets")]
public sealed class CampaignAssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public CampaignAssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AssetResponse>>>> FetchAvailableItems(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return Unauthorized(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "The access token does not contain a valid user identifier.",
                Data = null
            });
        }

        var result = await _assetService.GetAvailableItemsByCampaignIdAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Available campaign items fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<AssetResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<AssetResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can view campaign assets.",
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
}
