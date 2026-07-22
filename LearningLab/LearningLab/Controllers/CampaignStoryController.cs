using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Story;
using LearningLab.Services.CampaignStoryService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/content/story-blocks")]
public sealed class CampaignStoryController : ControllerBase
{
    private readonly ICampaignStoryService _campaignStoryService;

    public CampaignStoryController(ICampaignStoryService campaignStoryService)
    {
        _campaignStoryService = campaignStoryService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StoryBlockResponse>>> CreateStoryBlock(
        Guid campaignId,
        CreateStoryBlockRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBlockResponse>();
        }

        var result = await _campaignStoryService.CreateStoryBlockAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story block created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBlockResponse(
                result,
                "Story block created successfully.")
        };
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryBlockResponse>>>> FetchStoryBlocks(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryBlockResponse>>();
        }

        var result = await _campaignStoryService.GetStoryBlocksAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapStoryBlockListResponse(
            result,
            "Story blocks fetched successfully.");
    }

    [HttpPatch("{storyBlockId:guid}/title")]
    public async Task<ActionResult<ApiResponse<StoryBlockResponse>>> UpdateStoryBlockTitle(
        Guid campaignId,
        Guid storyBlockId,
        UpdateStoryBlockTitleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBlockResponse>();
        }

        var result = await _campaignStoryService.UpdateStoryBlockTitleAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return MapStoryBlockResponse(
            result,
            "Story block title updated successfully.");
    }

    [HttpDelete("{storyBlockId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStoryBlock(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignStoryService.DeleteStoryBlockAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Story block deleted successfully.",
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
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
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

    [HttpPost("{storyBlockId:guid}/beats/information")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> CreateInformationStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        CreateInformationStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.CreateInformationStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story beat created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatResponse(
                result,
                "Story beat created successfully.")
        };
    }

    [HttpPost("{storyBlockId:guid}/beats/narrative")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> CreateNarrativeStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        CreateNarrativeStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.CreateNarrativeStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story beat created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatResponse(
                result,
                "Story beat created successfully.")
        };
    }

    [HttpPost("{storyBlockId:guid}/beats/roleplaying")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> CreateRoleplayingStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        CreateRoleplayingStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.CreateRoleplayingStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story beat created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatResponse(
                result,
                "Story beat created successfully.")
        };
    }

    [HttpPost("{storyBlockId:guid}/beats/decision")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> CreateDecisionStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        CreateDecisionStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.CreateDecisionStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story beat created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatResponse(
                result,
                "Story beat created successfully.")
        };
    }

    [HttpPost("{storyBlockId:guid}/beats/milestone")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> CreateMilestoneStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        CreateMilestoneStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.CreateMilestoneStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Story beat created successfully.",
                Data = result.Data
            }),
            _ => MapStoryBeatResponse(
                result,
                "Story beat created successfully.")
        };
    }

    [HttpPut("{storyBlockId:guid}/beats/{storyBeatId:guid}/information")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> UpdateInformationStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateInformationStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.UpdateInformationStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            request,
            cancellationToken);

        return MapStoryBeatResponse(
            result,
            "Story beat updated successfully.");
    }

    [HttpPut("{storyBlockId:guid}/beats/{storyBeatId:guid}/narrative")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> UpdateNarrativeStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateNarrativeStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.UpdateNarrativeStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            request,
            cancellationToken);

        return MapStoryBeatResponse(
            result,
            "Story beat updated successfully.");
    }

    [HttpPut("{storyBlockId:guid}/beats/{storyBeatId:guid}/roleplaying")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> UpdateRoleplayingStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateRoleplayingStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.UpdateRoleplayingStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            request,
            cancellationToken);

        return MapStoryBeatResponse(
            result,
            "Story beat updated successfully.");
    }

    [HttpPut("{storyBlockId:guid}/beats/{storyBeatId:guid}/decision")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> UpdateDecisionStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateDecisionStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.UpdateDecisionStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            request,
            cancellationToken);

        return MapStoryBeatResponse(
            result,
            "Story beat updated successfully.");
    }

    [HttpPut("{storyBlockId:guid}/beats/{storyBeatId:guid}/milestone")]
    public async Task<ActionResult<ApiResponse<StoryBeatResponse>>> UpdateMilestoneStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        UpdateMilestoneStoryBeatRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryBeatResponse>();
        }

        var result = await _campaignStoryService.UpdateMilestoneStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            request,
            cancellationToken);

        return MapStoryBeatResponse(
            result,
            "Story beat updated successfully.");
    }

    [HttpDelete("{storyBlockId:guid}/beats/{storyBeatId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStoryBeat(
        Guid campaignId,
        Guid storyBlockId,
        Guid storyBeatId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignStoryService.DeleteStoryBeatAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            storyBeatId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Story beat deleted successfully.",
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
            ApplicationStatusCode.StoryBeatNotFound => NotFound(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
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

    [HttpGet("{storyBlockId:guid}/beats")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryBeatResponse>>>> FetchStoryBeats(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryBeatResponse>>();
        }

        var result = await _campaignStoryService.GetStoryBeatsAsync(
            userId.Value,
            campaignId,
            storyBlockId,
            cancellationToken);

        return MapStoryBeatListResponse(
            result,
            "Story beats fetched successfully.");
    }

    [HttpGet("roleplaying-npcs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignNpcResponse>>>>
        FetchRoleplayingStoryBeatNpcs(
            Guid campaignId,
            CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignNpcResponse>>();
        }

        var result = await _campaignStoryService.GetRoleplayingStoryBeatNpcsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapRoleplayingStoryBeatNpcListResponse(
            result,
            "Roleplaying story beat NPCs fetched successfully.");
    }

    [HttpPost("roleplaying-npcs")]
    public async Task<ActionResult<ApiResponse<CampaignNpcResponse>>> CreateCampaignNpc(
        Guid campaignId,
        CreateCampaignNpcRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignNpcResponse>();
        }

        var result = await _campaignStoryService.CreateCampaignNpcAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign NPC created successfully.",
                Data = result.Data
            }),
            _ => MapCampaignNpcResponse(
                result,
                "Campaign NPC created successfully.")
        };
    }

    [HttpPut("roleplaying-npcs/{npcTag}")]
    public async Task<ActionResult<ApiResponse<CampaignNpcResponse>>> UpdateCampaignNpc(
        Guid campaignId,
        string npcTag,
        UpdateCampaignNpcRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignNpcResponse>();
        }

        var result = await _campaignStoryService.UpdateCampaignNpcAsync(
            userId.Value,
            campaignId,
            npcTag,
            request,
            cancellationToken);

        return MapCampaignNpcResponse(
            result,
            "Campaign NPC updated successfully.");
    }

    private ActionResult<ApiResponse<IReadOnlyList<StoryBlockResponse>>> MapStoryBlockListResponse(
        ServiceResult<IReadOnlyList<StoryBlockResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoryBlockResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBlockResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBlockResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<StoryBlockResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<StoryBlockResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<StoryBlockResponse>> MapStoryBlockResponse(
        ServiceResult<StoryBlockResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidStoryBlock => BadRequest(new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Story block request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<StoryBlockResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<StoryBlockResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<StoryBlockResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>> MapStoryBlockMilestoneListResponse(
        ServiceResult<IReadOnlyList<StoryBlockMilestoneResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<StoryBlockMilestoneResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<StoryBlockMilestoneResponse>> MapStoryBlockMilestoneResponse(
        ServiceResult<StoryBlockMilestoneResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMilestoneNotFound => NotFound(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign milestone was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockMilestoneAlreadyExists => Conflict(new ApiResponse<StoryBlockMilestoneResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Campaign milestone is already included in this story block.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<StoryBlockMilestoneResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<StoryBlockMilestoneResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>> MapCampaignMilestoneListResponse(
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
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignMilestoneResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
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

    private ActionResult<ApiResponse<IReadOnlyList<StoryBeatResponse>>> MapStoryBeatListResponse(
        ServiceResult<IReadOnlyList<StoryBeatResponse>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<StoryBeatResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<IReadOnlyList<StoryBeatResponse>>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<StoryBeatResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<StoryBeatResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<IReadOnlyList<CampaignNpcResponse>>>
        MapRoleplayingStoryBeatNpcListResponse(
            ServiceResult<IReadOnlyList<CampaignNpcResponse>> result,
            string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<CampaignNpcResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignNpcResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<IReadOnlyList<CampaignNpcResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<CampaignNpcResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<IReadOnlyList<CampaignNpcResponse>>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<CampaignNpcResponse>> MapCampaignNpcResponse(
        ServiceResult<CampaignNpcResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCampaignNpc => BadRequest(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Campaign NPC request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNpcNotFound => NotFound(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign NPC was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNpcAlreadyExists => Conflict(new ApiResponse<CampaignNpcResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Campaign NPC tag already exists.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<CampaignNpcResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CampaignNpcResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private ActionResult<ApiResponse<StoryBeatResponse>> MapStoryBeatResponse(
        ServiceResult<StoryBeatResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidStoryBeat => BadRequest(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Story beat request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBlockNotFound => NotFound(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story block was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatNotFound => NotFound(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Story beat was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMilestoneNotFound => NotFound(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign milestone was not found.",
                Data = null
            }),
            ApplicationStatusCode.StoryBeatMilestoneAlreadyExists => Conflict(new ApiResponse<StoryBeatResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Campaign milestone is already linked to a story beat.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<StoryBeatResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign story content.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<StoryBeatResponse>
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
