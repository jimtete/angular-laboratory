using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Services.CampaignParticipationInviteService;
using LearningLab.Services.CampaignSettingsService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/users")]
public sealed class CampaignUsersController : ControllerBase
{
    private readonly ICampaignParticipationInviteService _campaignParticipationInviteService;
    private readonly ICampaignSettingsService _campaignSettingsService;

    public CampaignUsersController(
        ICampaignParticipationInviteService campaignParticipationInviteService,
        ICampaignSettingsService campaignSettingsService)
    {
        _campaignParticipationInviteService = campaignParticipationInviteService;
        _campaignSettingsService = campaignSettingsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CampaignUsernamesResponse>>> FetchCampaignUsernames(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignUsernamesResponse>();
        }

        var joinedMembersResult = await _campaignParticipationInviteService.GetCampaignMemberInformationAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        if (joinedMembersResult.StatusCode != ApplicationStatusCode.Success)
        {
            return MapCampaignUsernamesResponse(joinedMembersResult.StatusCode);
        }

        var invitedUsernamesResult = await _campaignParticipationInviteService.GetCampaignInviteUsernamesAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        if (invitedUsernamesResult.StatusCode != ApplicationStatusCode.Success)
        {
            return MapCampaignUsernamesResponse(invitedUsernamesResult.StatusCode);
        }

        return Ok(new ApiResponse<CampaignUsernamesResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Campaign users fetched successfully.",
            Data = new CampaignUsernamesResponse
            {
                CampaignId = campaignId,
                JoinedMembers = joinedMembersResult.Data ?? [],
                JoinedUsernames = joinedMembersResult.Data?.Select(member => member.Username).ToList() ?? [],
                InvitedUsernames = invitedUsernamesResult.Data ?? []
            }
        });
    }

    [HttpPut("{username}/skills")]
    public async Task<ActionResult<ApiResponse<CampaignMemberInformationResponse>>> UpdateCampaignMemberSkills(
        Guid campaignId,
        string username,
        UpdateCampaignMemberSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignMemberInformationResponse>();
        }

        var result = await _campaignSettingsService.UpdateCampaignMemberSkillsAsync(
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
                Message = "Campaign member skills updated successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignMemberSkills => BadRequest(
                new ApiResponse<CampaignMemberInformationResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign member skills are invalid.",
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
                    Message = "Only users with the Master role can manage campaign users.",
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

    private ActionResult<ApiResponse<CampaignUsernamesResponse>> MapCampaignUsernamesResponse(
        ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignUsernamesResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignUsernamesResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignUsernamesResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign users.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignUsernamesResponse>
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
