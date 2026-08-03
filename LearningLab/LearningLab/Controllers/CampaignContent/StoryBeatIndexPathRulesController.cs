using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Services.Helpers;
using LearningLab.Services.StoryBeatIndexPathRuleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/story-blocks/{storyBlockId:guid}/story-beat-index-path-rules")]
public sealed class StoryBeatIndexPathRulesController : ControllerBase
{
    private readonly IStoryBeatIndexPathRuleService _storyBeatIndexPathRuleService;

    public StoryBeatIndexPathRulesController(
        IStoryBeatIndexPathRuleService storyBeatIndexPathRuleService)
    {
        _storyBeatIndexPathRuleService = storyBeatIndexPathRuleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>>> ListPathRules(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>();
        }

        var result = await _storyBeatIndexPathRuleService.ListByStoryBlockAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            cancellationToken);

        return MapPathRuleListResponse(
            result,
            "Story beat index path rules fetched successfully.");
    }

    [HttpGet("{orderIndex:int}")]
    public async Task<ActionResult<ApiResponse<StoryBeatIndexPathRuleResponse>>> GetPathRule(
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatIndexPathRuleResponse>();
        }

        var result = await _storyBeatIndexPathRuleService.GetByOrderIndexAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            orderIndex,
            cancellationToken);

        return MapPathRuleResponse(
            result,
            "Story beat index path rule fetched successfully.");
    }

    [HttpPut("{orderIndex:int}")]
    public async Task<ActionResult<ApiResponse<StoryBeatIndexPathRuleResponse>>> UpsertPathRule(
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        UpsertStoryBeatIndexPathRuleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatIndexPathRuleResponse>();
        }

        var result = await _storyBeatIndexPathRuleService.UpsertAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            orderIndex,
            request,
            cancellationToken);

        return MapPathRuleResponse(
            result,
            "Story beat index path rule saved successfully.");
    }

    [HttpDelete("{orderIndex:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePathRule(
        Guid campaignId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _storyBeatIndexPathRuleService.DeleteAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            orderIndex,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Story beat index path rule deleted successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidStoryBeat => BadRequest(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Story beat order index is invalid.",
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
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatIndexPathRuleNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat index path rule was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatIndexPathRuleStoryBlockMismatch => Conflict(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Story block does not belong to this campaign.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage story beat index path rules.",
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

    private ActionResult<ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>> MapPathRuleListResponse(
        ServiceResult<IReadOnlyList<StoryBeatIndexPathRuleResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatIndexPathRuleStoryBlockMismatch => Conflict(
                new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Story block does not belong to this campaign.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage story beat index path rules.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<StoryBeatIndexPathRuleResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<StoryBeatIndexPathRuleResponse>> MapPathRuleResponse(
        ServiceResult<StoryBeatIndexPathRuleResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoryBeatIndexPathRuleResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidStoryBeat => BadRequest(new ApiResponse<StoryBeatIndexPathRuleResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Story beat order index is invalid.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatIndexPathRuleInvalidRelationType => BadRequest(
                new ApiResponse<StoryBeatIndexPathRuleResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Story beat index path rule relation type is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.StoryBeatIndexPathRuleRequiresMultipleStoryBeats => BadRequest(
                new ApiResponse<StoryBeatIndexPathRuleResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Story beat index path rules require at least two story beats at the same order index.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<StoryBeatIndexPathRuleResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<StoryBeatIndexPathRuleResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<StoryBeatIndexPathRuleResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatIndexPathRuleNotFound => NotFound(
                new ApiResponse<StoryBeatIndexPathRuleResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Story beat index path rule was not found.",
                    Data = null
                }),
            ApplicationStatusCode.StoryBeatIndexPathRuleStoryBlockMismatch => Conflict(
                new ApiResponse<StoryBeatIndexPathRuleResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Story block does not belong to this campaign.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<StoryBeatIndexPathRuleResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage story beat index path rules.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<StoryBeatIndexPathRuleResponse>
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
