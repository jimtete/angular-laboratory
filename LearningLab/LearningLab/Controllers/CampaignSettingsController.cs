using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Services.CampaignSettingsService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/settings")]
public sealed class CampaignSettingsController : ControllerBase
{
    private readonly ICampaignSettingsService _campaignSettingsService;

    public CampaignSettingsController(ICampaignSettingsService campaignSettingsService)
    {
        _campaignSettingsService = campaignSettingsService;
    }

    [HttpGet("information")]
    public async Task<ActionResult<ApiResponse<CampaignInformationResponse>>> FetchCampaignInformation(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignInformationResponse>();
        }

        var result = await _campaignSettingsService.GetCampaignInformationAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignInformationResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign information fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignInformationResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignInformationResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignInformationResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can view campaign information.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignInformationResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CampaignSettingsResponse>>> FetchCampaignSettings(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignSettingsResponse>();
        }

        var result = await _campaignSettingsService.GetCampaignSettingsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign settings fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignSettingsResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign settings.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignSettingsResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<CampaignSettingsResponse>>> UpdateCampaignSettings(
        Guid campaignId,
        UpdateCampaignSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignSettingsResponse>();
        }

        var result = await _campaignSettingsService.UpdateCampaignSettingsAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign settings updated successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignSettings => BadRequest(
                new ApiResponse<CampaignSettingsResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign settings are invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignSettingsResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignSettingsResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign settings.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignSettingsResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPut("members/{username}/nickname")]
    public async Task<ActionResult<ApiResponse<CampaignMemberInformationResponse>>> UpdateCampaignMemberNickname(
        Guid campaignId,
        string username,
        UpdateCampaignMemberNicknameRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignMemberInformationResponse>();
        }

        var result = await _campaignSettingsService.UpdateCampaignMemberNicknameAsync(
            userId.Value,
            campaignId,
            username,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignMemberInformationResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign member nickname updated successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignMemberNickname => BadRequest(
                new ApiResponse<CampaignMemberInformationResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign member nickname is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignMemberInformationResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignMemberInformationResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignParticipantNotFound => NotFound(
                new ApiResponse<CampaignMemberInformationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign member was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignMemberInformationResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign members.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignMemberInformationResponse>
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
