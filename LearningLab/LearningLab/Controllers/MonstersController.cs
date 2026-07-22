using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Monsters;
using LearningLab.Services.MonsterService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/monsters")]
public sealed class MonstersController : ControllerBase
{
    private readonly IMonsterService _monsterService;

    public MonstersController(IMonsterService monsterService)
    {
        _monsterService = monsterService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonsterListResponse>>>> FetchMonsters(
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.GetMonstersAsync(cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MonsterListResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monsters fetched successfully.",
                Data = result.Data
            }),
            _ => UnexpectedResponse<IReadOnlyList<MonsterListResponse>>()
        };
    }

    [HttpGet("{monsterId:int}")]
    public async Task<ActionResult<ApiResponse<MonsterResponse>>> FetchMonster(
        int monsterId,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.GetMonsterByIdAsync(
            monsterId,
            cancellationToken);

        return MapMonsterResponse(
            result,
            "Monster fetched successfully.");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MonsterResponse>>> CreateMonster(
        CreateMonsterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.CreateMonsterAsync(
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Monster created successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonster => BadRequest(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster request is invalid.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterResponse>()
        };
    }

    [HttpPut("{monsterId:int}/basic-information")]
    public async Task<ActionResult<ApiResponse<MonsterResponse>>> UpdateMonsterBasicInformation(
        int monsterId,
        UpdateMonsterBasicInformationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.UpdateMonsterBasicInformationAsync(
            monsterId,
            request,
            cancellationToken);

        return MapMonsterResponse(
            result,
            "Monster basic information updated successfully.");
    }

    [HttpDelete("{monsterId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMonster(
        int monsterId,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.DeleteMonsterAsync(
            monsterId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster deleted successfully.",
                Data = true
            }),
            ApplicationStatusCode.InvalidMonster => BadRequest(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster request is invalid.",
                Data = false
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = false
            }),
            _ => UnexpectedResponse<bool>()
        };
    }

    [HttpPost("{monsterId:int}/features")]
    public async Task<ActionResult<ApiResponse<MonsterFeatureResponse>>> AddMonsterFeature(
        int monsterId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.AddMonsterFeatureAsync(
            monsterId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Monster feature created successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonsterFeature => BadRequest(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster feature request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterFeatureResponse>()
        };
    }

    [HttpPut("{monsterId:int}/features/{featureId:int}")]
    public async Task<ActionResult<ApiResponse<MonsterFeatureResponse>>> UpdateMonsterFeature(
        int monsterId,
        int featureId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.UpdateMonsterFeatureAsync(
            monsterId,
            featureId,
            request,
            cancellationToken);

        return MapMonsterFeatureResponse(
            result,
            "Monster feature updated successfully.");
    }

    [HttpDelete("{monsterId:int}/features/{featureId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMonsterFeature(
        int monsterId,
        int featureId,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.DeleteMonsterFeatureAsync(
            monsterId,
            featureId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster feature deleted successfully.",
                Data = true
            }),
            ApplicationStatusCode.InvalidMonsterFeature => BadRequest(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster feature request is invalid.",
                Data = false
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = false
            }),
            ApplicationStatusCode.MonsterFeatureNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster feature was not found.",
                Data = false
            }),
            _ => UnexpectedResponse<bool>()
        };
    }

    [HttpPut("{monsterId:int}/features/reorder")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonsterFeatureResponse>>>> ReorderMonsterFeatures(
        int monsterId,
        ReorderMonsterFeaturesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.ReorderMonsterFeaturesAsync(
            monsterId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<MonsterFeatureResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster features reordered successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonsterFeature => BadRequest(
                new ApiResponse<IReadOnlyList<MonsterFeatureResponse>>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Monster feature reorder request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.MonsterNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterFeatureResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Monster was not found.",
                    Data = null
                }),
            ApplicationStatusCode.MonsterFeatureNotFound => NotFound(
                new ApiResponse<IReadOnlyList<MonsterFeatureResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "One or more monster features were not found.",
                    Data = null
                }),
            _ => UnexpectedResponse<IReadOnlyList<MonsterFeatureResponse>>()
        };
    }

    [HttpPut("{monsterId:int}/spellcasting")]
    public async Task<ActionResult<ApiResponse<MonsterSpellcastingResponse>>> UpsertMonsterSpellcasting(
        int monsterId,
        MonsterSpellcastingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.UpsertMonsterSpellcastingAsync(
            monsterId,
            request,
            cancellationToken);

        return MapMonsterSpellcastingResponse(
            result,
            "Monster spellcasting saved successfully.");
    }

    [HttpDelete("{monsterId:int}/spellcasting")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveMonsterSpellcasting(
        int monsterId,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.RemoveMonsterSpellcastingAsync(
            monsterId,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster spellcasting removed successfully.",
                Data = true
            }),
            ApplicationStatusCode.InvalidMonsterSpellcasting => BadRequest(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster spellcasting request is invalid.",
                Data = false
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = false
            }),
            ApplicationStatusCode.MonsterSpellcastingNotFound => NotFound(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster spellcasting was not found.",
                Data = false
            }),
            _ => UnexpectedResponse<bool>()
        };
    }

    [HttpPatch("{monsterId:int}/spellcasting/slots/{spellLevel:int}/remaining")]
    public async Task<ActionResult<ApiResponse<MonsterSpellcastingResponse>>> UpdateRemainingSpellSlots(
        int monsterId,
        int spellLevel,
        UpdateRemainingSpellSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _monsterService.UpdateRemainingSpellSlotsAsync(
            monsterId,
            spellLevel,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MonsterSpellcastingResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Monster spell slot remaining count updated successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonsterSpellSlot => BadRequest(new ApiResponse<MonsterSpellcastingResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster spell slot request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterSpellcastingResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            ApplicationStatusCode.MonsterSpellcastingNotFound => NotFound(
                new ApiResponse<MonsterSpellcastingResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Monster spellcasting was not found.",
                    Data = null
                }),
            ApplicationStatusCode.MonsterSpellSlotNotFound => NotFound(
                new ApiResponse<MonsterSpellcastingResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Monster spell slot was not found.",
                    Data = null
                }),
            _ => UnexpectedResponse<MonsterSpellcastingResponse>()
        };
    }

    private ActionResult<ApiResponse<MonsterResponse>> MapMonsterResponse(
        ServiceResult<MonsterResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonster => BadRequest(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterResponse>()
        };
    }

    private ActionResult<ApiResponse<MonsterFeatureResponse>> MapMonsterFeatureResponse(
        ServiceResult<MonsterFeatureResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonsterFeature => BadRequest(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Monster feature request is invalid.",
                Data = null
            }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            ApplicationStatusCode.MonsterFeatureNotFound => NotFound(new ApiResponse<MonsterFeatureResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster feature was not found.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterFeatureResponse>()
        };
    }

    private ActionResult<ApiResponse<MonsterSpellcastingResponse>> MapMonsterSpellcastingResponse(
        ServiceResult<MonsterSpellcastingResponse> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<MonsterSpellcastingResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidMonsterSpellcasting => BadRequest(
                new ApiResponse<MonsterSpellcastingResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Monster spellcasting request is invalid.",
                    Data = null
                }),
            ApplicationStatusCode.MonsterNotFound => NotFound(new ApiResponse<MonsterSpellcastingResponse>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Monster was not found.",
                Data = null
            }),
            _ => UnexpectedResponse<MonsterSpellcastingResponse>()
        };
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
