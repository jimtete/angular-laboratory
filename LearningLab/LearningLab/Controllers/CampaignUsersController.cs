using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Services.CampaignParticipationInviteService;
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

    public CampaignUsersController(ICampaignParticipationInviteService campaignParticipationInviteService)
    {
        _campaignParticipationInviteService = campaignParticipationInviteService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CampaignUsernamesResponse>>> FetchCampaignUsernames(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse();
        }

        var joinedUsernamesResult = await _campaignParticipationInviteService.GetCampaignMemberUsernamesAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        if (joinedUsernamesResult.StatusCode != ApplicationStatusCode.Success)
        {
            return MapCampaignUsernamesResponse(joinedUsernamesResult.StatusCode);
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
                JoinedUsernames = joinedUsernamesResult.Data ?? [],
                InvitedUsernames = invitedUsernamesResult.Data ?? []
            }
        });
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

    private UnauthorizedObjectResult InvalidUserClaimResponse()
    {
        return Unauthorized(new ApiResponse<CampaignUsernamesResponse>
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            Message = "The access token does not contain a valid user identifier.",
            Data = null
        });
    }
}
