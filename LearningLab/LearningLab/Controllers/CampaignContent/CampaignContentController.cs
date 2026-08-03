using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Services.CampaignContentService;
using LearningLab.Services.CampaignQuestService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/content")]
public sealed class CampaignContentController : ControllerBase
{
    private readonly ICampaignContentService _campaignContentService;
    private readonly ICampaignQuestService _campaignQuestService;

    public CampaignContentController(
        ICampaignContentService campaignContentService,
        ICampaignQuestService campaignQuestService)
    {
        _campaignContentService = campaignContentService;
        _campaignQuestService = campaignQuestService;
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

    [HttpGet("quests")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignQuestResponse>>>> FetchCampaignQuests(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignQuestResponse>>();
        }

        var result = await _campaignQuestService.GetCampaignQuestsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapQuestListResponse(
            result,
            "Campaign quests fetched successfully.");
    }

    [HttpPost("quests")]
    public async Task<ActionResult<ApiResponse<CampaignQuestResponse>>> CreateCampaignQuest(
        Guid campaignId,
        CreateCampaignQuestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignQuestResponse>();
        }

        var result = await _campaignQuestService.CreateCampaignQuestAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign quest created successfully.",
                Data = result.Data
            }),
            _ => MapQuestResponse(
                result,
                "Campaign quest created successfully.")
        };
    }

    [HttpPut("quests/{questId:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignQuestResponse>>> UpdateCampaignQuest(
        Guid campaignId,
        Guid questId,
        UpdateCampaignQuestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignQuestResponse>();
        }

        var result = await _campaignQuestService.UpdateCampaignQuestAsync(
            userId.Value,
            campaignId,
            questId,
            request,
            cancellationToken);

        return MapQuestResponse(
            result,
            "Campaign quest updated successfully.");
    }

    [HttpDelete("quests/{questId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>>> DeleteCampaignQuest(
        Guid campaignId,
        Guid questId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>();
        }

        var result = await _campaignQuestService.DeleteCampaignQuestAsync(
            userId.Value,
            campaignId,
            questId,
            cancellationToken);

        return MapQuestDeleteResponse(
            result,
            "Campaign quest deleted successfully.");
    }

    [HttpGet("quest-tasks/story-beat-links")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>>> FetchCampaignStoryBeatQuestTasks(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>();
        }

        var result = await _campaignQuestService.GetCampaignStoryBeatQuestTasksAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapStoryBeatQuestTaskListResponse(
            result,
            "Campaign story beat quest task links fetched successfully.");
    }

    [HttpGet("story-beats/{storyBeatId:guid}/quest-tasks")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>>> FetchStoryBeatQuestTasks(
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>();
        }

        var result = await _campaignQuestService.GetStoryBeatQuestTasksAsync(
            userId.Value,
            campaignId,
            storyBeatId,
            cancellationToken);

        return MapStoryBeatQuestTaskListResponse(
            result,
            "Story beat quest tasks fetched successfully.");
    }

    [HttpPost("story-beats/{storyBeatId:guid}/quest-tasks/{questTaskId:guid}")]
    public async Task<ActionResult<ApiResponse<StoryBeatQuestTaskResponse>>> LinkQuestTaskToStoryBeat(
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatQuestTaskResponse>();
        }

        var result = await _campaignQuestService.LinkQuestTaskToStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBeatId,
            questTaskId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Quest task linked to story beat successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatQuestTaskResponse(
                result,
                "Quest task linked to story beat successfully.")
        };
    }

    [HttpDelete("story-beats/{storyBeatId:guid}/quest-tasks/{questTaskId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UnlinkQuestTaskFromStoryBeat(
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignQuestService.UnlinkQuestTaskFromStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBeatId,
            questTaskId,
            cancellationToken);

        return MapStoryBeatQuestTaskObjectResponse(
            result,
            "Quest task unlinked from story beat successfully.");
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

    private ActionResult<ApiResponse<IReadOnlyList<CampaignQuestResponse>>> MapQuestListResponse(
        ServiceResult<IReadOnlyList<CampaignQuestResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignQuestResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<CampaignQuestResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<CampaignQuestResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignQuestResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignQuestResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<CampaignQuestResponse>> MapQuestResponse(
        ServiceResult<CampaignQuestResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignQuest => BadRequest(new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign quest request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignQuestNotFound => NotFound(new ApiResponse<CampaignQuestResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign quest was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignQuestResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignQuestResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>> MapQuestDeleteResponse(
        ServiceResult<IReadOnlyList<CampaignQuestDeleteBlockerResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = null
            }),
            ApplicationStatusCode.InvalidCampaignQuest => BadRequest(
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign quest request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignQuestNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign quest was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignQuestDeleteBlocked => Conflict(
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Campaign quest cannot be deleted because it is used by other campaign content.",
                    Data = result.Data
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignQuestDeleteBlockerResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>> MapStoryBeatQuestTaskListResponse(
        ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignQuestTask => BadRequest(
                new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign quest task request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<StoryBeatQuestTaskResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<StoryBeatQuestTaskResponse>> MapStoryBeatQuestTaskResponse(
        ServiceResult<StoryBeatQuestTaskResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignQuestTask => BadRequest(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign quest task request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatNotFound => NotFound(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignQuestTaskNotFound => NotFound(new ApiResponse<StoryBeatQuestTaskResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign quest task was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatQuestTaskAlreadyExists => Conflict(
                new ApiResponse<StoryBeatQuestTaskResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Campaign quest task is already linked to this story beat.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignQuestTaskAlreadyAssignedToStoryBeat => Conflict(
                new ApiResponse<StoryBeatQuestTaskResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Campaign quest task is already linked to another story beat in this campaign.",
                    Data = result.Data
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<StoryBeatQuestTaskResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<StoryBeatQuestTaskResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<object>> MapStoryBeatQuestTaskObjectResponse(
        ServiceResult<object> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = null
            }),
            ApplicationStatusCode.InvalidCampaignQuestTask => BadRequest(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign quest task request is invalid.",
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
            ApplicationStatusCode.StoryBeatNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignQuestTaskNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign quest task was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatQuestTaskNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign quest task is not linked to this story beat.",
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
