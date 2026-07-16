using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Services.CampaignSessionService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/sessions")]
public sealed class CampaignSessionsController : ControllerBase
{
    private readonly ICampaignSessionService _campaignSessionService;

    public CampaignSessionsController(ICampaignSessionService campaignSessionService)
    {
        _campaignSessionService = campaignSessionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignSessionResponse>>>> FetchCampaignSessions(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignSessionResponse>>();
        }

        var result = await _campaignSessionService.GetAvailableCampaignSessionsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignSessionResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign sessions fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignSessionResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignSessionResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignSessionResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can view campaign sessions.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignSessionResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampaignSessionResponse>>> CreateCampaignSession(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignSessionResponse>();
        }

        var result = await _campaignSessionService.CreateCampaignSessionAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignSessionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign session created successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<CampaignSessionResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<CampaignSessionResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignSessionResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can create campaign sessions.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignSessionResponse>
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
