using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Services.CampaignContentService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/content")]
public sealed class CampaignContentController : ControllerBase
{
    private readonly ICampaignContentService _campaignContentService;

    public CampaignContentController(ICampaignContentService campaignContentService)
    {
        _campaignContentService = campaignContentService;
    }

    [HttpGet("milestones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>>> FetchCampaignMilestones(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignMilestoneResponse>>();
        }

        var result = await _campaignContentService.GetCampaignMilestonesAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapMilestoneListResponse(
            result,
            "Campaign milestones fetched successfully.");
    }

    [HttpPost("milestones")]
    public async Task<ActionResult<ApiResponse<CampaignMilestoneResponse>>> CreateCampaignMilestone(
        Guid campaignId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignMilestoneResponse>();
        }

        var result = await _campaignContentService.CreateCampaignMilestoneAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign milestone created successfully.",
                Data = result.Data
            }),
            _ => MapMilestoneResponse(
                result,
                "Campaign milestone created successfully.")
        };
    }

    [HttpGet("milestones/unachieved")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>>> FetchUnachievedCampaignMilestones(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignMilestoneResponse>>();
        }

        var result = await _campaignContentService.GetUnachievedCampaignMilestonesAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapMilestoneListResponse(
            result,
            "Unachieved campaign milestones fetched successfully.");
    }

    [HttpPut("milestones/{milestoneId:int}")]
    public async Task<ActionResult<ApiResponse<CampaignMilestoneResponse>>> UpdateCampaignMilestone(
        Guid campaignId,
        int milestoneId,
        CampaignMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignMilestoneResponse>();
        }

        var result = await _campaignContentService.UpdateCampaignMilestoneAsync(
            userId.Value,
            campaignId,
            milestoneId,
            request,
            cancellationToken);

        return MapMilestoneResponse(
            result,
            "Campaign milestone updated successfully.");
    }

    [HttpDelete("milestones/{milestoneId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCampaignMilestone(
        Guid campaignId,
        int milestoneId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignContentService.DeleteCampaignMilestoneAsync(
            userId.Value,
            campaignId,
            milestoneId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign milestone deleted successfully.",
                Data = null
            }),
            ApplicationStatusCode.InvalidCampaignMilestone => BadRequest(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign milestone request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMilestoneNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign milestone was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>> MapMilestoneListResponse(
        ServiceResult<IReadOnlyList<CampaignMilestoneResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<CampaignMilestoneResponse>> MapMilestoneResponse(
        ServiceResult<CampaignMilestoneResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignMilestone => BadRequest(new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign milestone request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMilestoneNotFound => NotFound(new ApiResponse<CampaignMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign milestone was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignMilestoneResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignMilestoneResponse>
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
