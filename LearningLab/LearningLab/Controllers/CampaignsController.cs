using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Parsers;
using LearningLab.Services.CampaignParticipationInviteService;
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
    private readonly ICampaignParticipationInviteService _campaignParticipationInviteService;
    private readonly ICampaignService _campaignService;

    public CampaignsController(
        ICampaignParticipationInviteService campaignParticipationInviteService,
        ICampaignService campaignService)
    {
        _campaignParticipationInviteService = campaignParticipationInviteService;
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
                Data = result.Data?.Select(WithPublicAssetUrls).ToList()
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CampaignResponse>>> CreateCampaign(
        [FromForm] CreateCampaignRequest request,
        [FromForm] IFormFile? campaignPicture,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignResponse>();
        }

        var campaignPictureBytes = campaignPicture is null
            ? null
            : await MediaParser.ReadCampaignPictureBytesAsync(
                campaignPicture,
                cancellationToken);

        var result = await _campaignService.CreateCampaignAsync(
            userId.Value,
            request,
            campaignPictureBytes,
            campaignPicture?.ContentType,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign created successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrls(result.Data)
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
            ApplicationStatusCode.CampaignPictureTooLarge => StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new ApiResponse<CampaignResponse>
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge,
                    Message = "Campaign picture must be 5 MB or smaller.",
                    Data = null
                }),
            ApplicationStatusCode.UnsupportedCampaignPictureFormat => StatusCode(
                StatusCodes.Status415UnsupportedMediaType,
                new ApiResponse<CampaignResponse>
                {
                    StatusCode = StatusCodes.Status415UnsupportedMediaType,
                    Message = "Campaign picture must be a JPEG image.",
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

    [HttpPost("{campaignId:guid}/invites")]
    [Authorize(Roles = AccessRoleNames.Master)]
    public async Task<ActionResult<ApiResponse<CampaignParticipationInviteResponse>>> InvitePlayer(
        Guid campaignId,
        CreateCampaignParticipationInviteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignParticipationInviteResponse>();
        }

        var result = await _campaignParticipationInviteService.InvitePlayerAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignParticipationInviteResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Player invited successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignInvite => BadRequest(
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign invite request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignParticipationInviteResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignParticipationInviteResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can invite players to campaigns.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignInvitePlayerRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Player role can be invited to campaigns.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignInviteAlreadyExists => Conflict(
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "The player already has a pending campaign invite.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignParticipantAlreadyExists => Conflict(
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "The player already exists in this campaign.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignParticipationInviteResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private CampaignResponse WithPublicAssetUrls(CampaignResponse campaign)
    {
        return new CampaignResponse
        {
            CampaignId = campaign.CampaignId,
            GameMasterId = campaign.GameMasterId,
            GameMasterUsername = campaign.GameMasterUsername,
            CampaignName = campaign.CampaignName,
            Version = campaign.Version,
            CampaignPictureUrl = Request.ToPublicStaticAssetUrl(campaign.CampaignPictureUrl),
            DateCreated = campaign.DateCreated
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
