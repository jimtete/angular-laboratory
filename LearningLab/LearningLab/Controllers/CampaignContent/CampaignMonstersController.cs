using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Monsters;
using LearningLab.Services.Helpers;
using LearningLab.Services.MonsterService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers.CampaignContent;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaigns/{campaignId:guid}/monsters")]
public sealed class CampaignMonstersController : ControllerBase
{
    private readonly IMonsterService _monsterService;

    public CampaignMonstersController(IMonsterService monsterService)
    {
        _monsterService = monsterService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonsterListResponse>>>> FetchCampaignMonsters(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<MonsterListResponse>>();
        }

        var result = await _monsterService.GetCampaignMonstersAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MonsterListResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign monsters fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterListResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterListResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<MonsterListResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can view campaign monsters.",
                    Data = null
                }),
            _ => UnexpectedResponse<IReadOnlyList<MonsterListResponse>>()
        };
    }

    [HttpGet("details")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonsterResponse>>>> FetchCampaignMonsterDetails(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<MonsterResponse>>();
        }

        var result = await _monsterService.GetCampaignMonsterDetailsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MonsterResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Campaign monster details fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UserNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Campaign was not found.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<IReadOnlyList<MonsterResponse>>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can view campaign monsters.",
                    Data = null
                }),
            _ => UnexpectedResponse<IReadOnlyList<MonsterResponse>>()
        };
    }

    [HttpPost("{monsterId:int}")]
    public async Task<ActionResult<ApiResponse<MonsterResponse>>> AddMonsterToCampaign(
        Guid campaignId,
        int monsterId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<MonsterResponse>();
        }

        var result = await _monsterService.AddMonsterToCampaignAsync(
            userId.Value,
            campaignId,
            monsterId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Monster added to campaign successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonster => BadRequest(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = null
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<MonsterResponse>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can add monsters to campaigns.",
                    Data = null
                }),
            ApplicationStatusCode.CampaignMonsterAlreadyExists => Conflict(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Monster is already available in this campaign.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterListResponse>()
        };
    }

    [HttpDelete("{monsterId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveMonsterFromCampaign(
        Guid campaignId,
        int monsterId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<bool>();
        }

        var result = await _monsterService.RemoveMonsterFromCampaignAsync(
            userId.Value,
            campaignId,
            monsterId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster removed from campaign successfully.",
                Data = true
            }),
            ApplicationStatusCode.InvalidMonster => BadRequest(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster request is invalid.",
                Data = false
            }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = false
            }),
            ApplicationStatusCode.CampaignNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Campaign was not found.",
                Data = false
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = false
            }),
            ApplicationStatusCode.CampaignMonsterNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster is not available in this campaign.",
                Data = false
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<bool>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can remove monsters from campaigns.",
                    Data = false
                }),
            _ => UnexpectedResponse<bool>()
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

    private ObjectResult UnexpectedResponse<T>()
    {
        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                Data = default
            });
    }
}
