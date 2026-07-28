using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Campaign.Rules;
using LearningLab.Services.CampaignRulesService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize(Roles = AccessRoleNames.Master)]
[Route("api/campaign-sessions/{campaignSessionId:int}")]
public sealed class CampaignSessionRulesController : ControllerBase
{
    private readonly ICampaignRulesService _campaignRulesService;

    public CampaignSessionRulesController(ICampaignRulesService campaignRulesService)
    {
        _campaignRulesService = campaignRulesService;
    }

    [HttpGet("event-states")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignEventStateResponse>>>> GetEventStates(
        int campaignSessionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignEventStateResponse>>();
        }

        var result = await _campaignRulesService.GetEventStatesAsync(
            userId.Value,
            campaignSessionId,
            cancellationToken);

        return MapListResponse(result, "Campaign event states fetched successfully.");
    }

    [HttpPut("event-states/{eventDefinitionId:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignEventStateResponse>>> SetEventState(
        int campaignSessionId,
        Guid eventDefinitionId,
        CampaignEventStateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventStateResponse>();
        }

        var result = await _campaignRulesService.SetEventStateAsync(
            userId.Value,
            campaignSessionId,
            eventDefinitionId,
            request,
            cancellationToken);

        return MapResponse(result, "Campaign event state updated successfully.");
    }

    [HttpDelete("event-states/{eventDefinitionId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEventState(
        int campaignSessionId,
        Guid eventDefinitionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignRulesService.DeleteEventStateAsync(
            userId.Value,
            campaignSessionId,
            eventDefinitionId,
            cancellationToken);

        return MapObjectResponse(result, "Campaign event state deleted successfully.");
    }

    [HttpPost("rules/evaluate")]
    public async Task<ActionResult<ApiResponse<RuleEvaluationResult>>> EvaluateRule(
        int campaignSessionId,
        RuleEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<RuleEvaluationResult>();
        }

        if (request.RuleId is null)
        {
            return BadRequest(new ApiResponse<RuleEvaluationResult>
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Rule id is required.",
                Data = null
            });
        }

        var result = await _campaignRulesService.EvaluateRuleAsync(
            userId.Value,
            request.RuleId.Value,
            campaignSessionId,
            cancellationToken);

        return MapResponse(result, "Rule evaluated successfully.");
    }

    [HttpGet("targets/{targetType}/{targetId:guid}/availability")]
    public async Task<ActionResult<ApiResponse<TargetAvailabilityResult>>> GetTargetAvailability(
        int campaignSessionId,
        ConditionalTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<TargetAvailabilityResult>();
        }

        var result = await _campaignRulesService.IsTargetAvailableAsync(
            userId.Value,
            targetType,
            targetId,
            campaignSessionId,
            cancellationToken);

        return MapResponse(result, "Target availability evaluated successfully.");
    }

    [HttpPost("outcomes/apply")]
    public async Task<ActionResult<ApiResponse<ApplyOutcomeResult>>> ApplyOutcome(
        int campaignSessionId,
        ApplyOutcomeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<ApplyOutcomeResult>();
        }

        var result = await _campaignRulesService.ApplyOutcomeEffectsAsync(
            userId.Value,
            campaignSessionId,
            request.SourceType,
            request.SourceId,
            cancellationToken);

        return MapResponse(result, "Outcome effects applied successfully.");
    }

    [HttpGet("choice-selections")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignChoiceSelectionResponse>>>> GetChoiceSelections(
        int campaignSessionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignChoiceSelectionResponse>>();
        }

        var result = await _campaignRulesService.GetChoiceSelectionsAsync(
            userId.Value,
            campaignSessionId,
            cancellationToken);

        return MapListResponse(result, "Campaign choice selections fetched successfully.");
    }

    [HttpPost("choice-selections")]
    public async Task<ActionResult<ApiResponse<CampaignChoiceSelectionResponse>>> SelectChoiceOption(
        int campaignSessionId,
        SelectCampaignChoiceOptionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignChoiceSelectionResponse>();
        }

        var result = await _campaignRulesService.SelectChoiceOptionAsync(
            userId.Value,
            campaignSessionId,
            request,
            false,
            cancellationToken);

        return MapResponse(result, "Campaign choice option selected successfully.");
    }

    [HttpPut("choice-selections")]
    public async Task<ActionResult<ApiResponse<CampaignChoiceSelectionResponse>>> ChangeChoiceOption(
        int campaignSessionId,
        SelectCampaignChoiceOptionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignChoiceSelectionResponse>();
        }

        var result = await _campaignRulesService.SelectChoiceOptionAsync(
            userId.Value,
            campaignSessionId,
            request,
            true,
            cancellationToken);

        return MapResponse(result, "Campaign choice option changed successfully.");
    }

    private ActionResult<ApiResponse<IReadOnlyList<T>>> MapListResponse<T>(
        ServiceResult<IReadOnlyList<T>> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<IReadOnlyList<T>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            _ => MapFailure<IReadOnlyList<T>>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<T>> MapResponse<T>(
        ServiceResult<T> result,
        string successMessage)
    {
        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = successMessage,
                Data = result.Data
            }),
            _ => MapFailure<T>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<object>> MapObjectResponse(
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
            _ => MapFailure<object>(result.StatusCode)
        };
    }

    private ActionResult<ApiResponse<T>> MapFailure<T>(ApplicationStatusCode statusCode)
    {
        return statusCode switch
        {
            ApplicationStatusCode.InvalidCampaignEventState
                or ApplicationStatusCode.InvalidStoryOutcomeEffect => BadRequest(new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign runtime rule request is invalid.",
                    Data = default
                }),
            ApplicationStatusCode.UserNotFound
                or ApplicationStatusCode.CampaignNotFound
                or ApplicationStatusCode.CampaignSessionNotFound
                or ApplicationStatusCode.CampaignEventDefinitionNotFound
                or ApplicationStatusCode.CampaignEventStateNotFound
                or ApplicationStatusCode.ConditionalRuleNotFound
                or ApplicationStatusCode.CampaignChoiceOptionNotFound => NotFound(new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Requested campaign runtime rule resource was not found.",
                    Data = default
                }),
            ApplicationStatusCode.CampaignChoiceSelectionConflict => Conflict(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Campaign choice selection conflicts with the current selection mode.",
                Data = default
            }),
            ApplicationStatusCode.CampaignMasterRoleRequired => StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Only users with the Master role can manage campaign rules.",
                    Data = default
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = default
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

