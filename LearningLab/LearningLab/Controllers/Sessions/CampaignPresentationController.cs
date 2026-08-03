using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Hub;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.Sessions;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/sessions/{sessionId:int}/presentation")]
public sealed class CampaignPresentationController : ControllerBase
{
    private readonly IPresentationModeHub _presentationModeHub;

    public CampaignPresentationController(IPresentationModeHub presentationModeHub)
    {
        _presentationModeHub = presentationModeHub;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CampaignPresentationResponse>>> FetchPresentationMode(
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignPresentationResponse>();
        }

        var result = await _presentationModeHub.GetPresentationModeAsync(
            userId.Value,
            campaignId,
            sessionId,
            cancellationToken);

        return MapPresentationResponse(
            result,
            "Presentation mode fetched successfully.");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampaignPresentationResponse>>> InitiatePresentationMode(
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignPresentationResponse>();
        }

        var result = await _presentationModeHub.InitiatePresentationModeAsync(
            userId.Value,
            campaignId,
            sessionId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignPresentationResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Presentation mode initiated successfully.",
                Data = result.Data
            }),
            _ => MapPresentationResponse(
                result,
                "Presentation mode initiated successfully.")
        };
    }

    [HttpPost("beats")]
    public async Task<ActionResult<ApiResponse<CampaignPresentationResponse>>> PresentStoryBeat(
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignPresentationResponse>();
        }

        var result = await _presentationModeHub.PresentStoryBeatAsync(
            userId.Value,
            campaignId,
            sessionId,
            request,
            cancellationToken);

        return MapPresentationResponse(
            result,
            "Story beat presented successfully.");
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<CampaignPresentationResponse>>> DisablePresentationMode(
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignPresentationResponse>();
        }

        var result = await _presentationModeHub.DisablePresentationModeAsync(
            userId.Value,
            campaignId,
            sessionId,
            cancellationToken);

        return MapPresentationResponse(
            result,
            "Presentation mode disabled successfully.");
    }

    private ActionResult<ApiResponse<CampaignPresentationResponse>> MapPresentationResponse(
        ServiceResult<CampaignPresentationResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignPresentationResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignPresentation => BadRequest(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Presentation mode request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignSessionNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign session was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignPresentationNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Presentation mode has not been initiated for this session.",
                    Data = null
                }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Story block was not found.",
                    Data = null
                }),
            ApplicationStatusCode.StoryBeatNotFound => NotFound(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Story beat was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignPresentationStoryBeatConflict => Conflict(
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Another story beat has already been selected for this story beat index.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignPresentationResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage presentation mode.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignPresentationResponse>
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
