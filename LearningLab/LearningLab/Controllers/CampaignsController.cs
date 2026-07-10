using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Services.CampaignService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
[Route("api/[controller]")]
public sealed class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignResponse>>>> FetchAvailableCampaigns(
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignResponse>>();
        }

        var result = await _campaignService.GetAvailableCampaignsAsync(
            userId.Value,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaigns fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPost]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<CampaignResponse>>> CreateCampaign(
        CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignResponse>();
        }

        var result = await _campaignService.CreateCampaignAsync(
            userId.Value,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign created successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can create campaigns.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
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
