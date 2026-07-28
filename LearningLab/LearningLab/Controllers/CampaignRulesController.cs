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
[Route("api/campaigns/{campaignId:guid}")]
public sealed class CampaignRulesController : ControllerBase
{
    private readonly ICampaignRulesService _campaignRulesService;

    public CampaignRulesController(ICampaignRulesService campaignRulesService)
    {
        _campaignRulesService = campaignRulesService;
    }

    [HttpGet("event-definitions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignEventDefinitionResponse>>>> GetEventDefinitions(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<CampaignEventDefinitionResponse>>();
        }

        var result = await _campaignRulesService.GetEventDefinitionsAsync(
            userId.Value,
            campaignId,
            cancellationToken);

        return MapListResponse(result, "Campaign event definitions fetched successfully.");
    }

    [HttpGet("event-definitions/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignEventDefinitionResponse>>> GetEventDefinition(
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventDefinitionResponse>();
        }

        var result = await _campaignRulesService.GetEventDefinitionAsync(
            userId.Value,
            campaignId,
            id,
            cancellationToken);

        return MapResponse(result, "Campaign event definition fetched successfully.");
    }

    [HttpPost("event-definitions")]
    public async Task<ActionResult<ApiResponse<CampaignEventDefinitionResponse>>> CreateEventDefinition(
        Guid campaignId,
        CampaignEventDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventDefinitionResponse>();
        }

        var result = await _campaignRulesService.CreateEventDefinitionAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignEventDefinitionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign event definition created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Campaign event definition created successfully.")
        };
    }

    [HttpPut("event-definitions/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignEventDefinitionResponse>>> UpdateEventDefinition(
        Guid campaignId,
        Guid id,
        CampaignEventDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventDefinitionResponse>();
        }

        var result = await _campaignRulesService.UpdateEventDefinitionAsync(
            userId.Value,
            campaignId,
            id,
            request,
            cancellationToken);

        return MapResponse(result, "Campaign event definition updated successfully.");
    }

    [HttpDelete("event-definitions/{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEventDefinition(
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignRulesService.DeleteEventDefinitionAsync(
            userId.Value,
            campaignId,
            id,
            cancellationToken);

        return MapObjectResponse(result, "Campaign event definition deleted successfully.");
    }

    [HttpPost("event-definitions/{eventDefinitionId:guid}/options")]
    public async Task<ActionResult<ApiResponse<CampaignEventOptionResponse>>> CreateEventOption(
        Guid campaignId,
        Guid eventDefinitionId,
        CampaignEventOptionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventOptionResponse>();
        }

        var result = await _campaignRulesService.CreateEventOptionAsync(
            userId.Value,
            campaignId,
            eventDefinitionId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignEventOptionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign event option created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Campaign event option created successfully.")
        };
    }

    [HttpPut("event-definitions/{eventDefinitionId:guid}/options/{optionId:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignEventOptionResponse>>> UpdateEventOption(
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CampaignEventOptionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignEventOptionResponse>();
        }

        var result = await _campaignRulesService.UpdateEventOptionAsync(
            userId.Value,
            campaignId,
            eventDefinitionId,
            optionId,
            request,
            cancellationToken);

        return MapResponse(result, "Campaign event option updated successfully.");
    }

    [HttpDelete("event-definitions/{eventDefinitionId:guid}/options/{optionId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEventOption(
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignRulesService.DeleteEventOptionAsync(
            userId.Value,
            campaignId,
            eventDefinitionId,
            optionId,
            cancellationToken);

        return MapObjectResponse(result, "Campaign event option deleted successfully.");
    }

    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConditionalRuleResponse>>>> GetRulesForTarget(
        Guid campaignId,
        [FromQuery] ConditionalTargetType targetType,
        [FromQuery] Guid targetId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<ConditionalRuleResponse>>();
        }

        var result = await _campaignRulesService.GetRulesForTargetAsync(
            userId.Value,
            campaignId,
            targetType,
            targetId,
            cancellationToken);

        return MapListResponse(result, "Conditional rules fetched successfully.");
    }

    [HttpPost("rules")]
    public async Task<ActionResult<ApiResponse<ConditionalRuleResponse>>> CreateRule(
        Guid campaignId,
        ConditionalRuleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<ConditionalRuleResponse>();
        }

        var result = await _campaignRulesService.CreateRuleAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<ConditionalRuleResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Conditional rule created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Conditional rule created successfully.")
        };
    }

    [HttpPut("rules/{ruleId:guid}")]
    public async Task<ActionResult<ApiResponse<ConditionalRuleResponse>>> UpdateRule(
        Guid campaignId,
        Guid ruleId,
        ConditionalRuleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<ConditionalRuleResponse>();
        }

        var result = await _campaignRulesService.UpdateRuleAsync(
            userId.Value,
            campaignId,
            ruleId,
            request,
            cancellationToken);

        return MapResponse(result, "Conditional rule updated successfully.");
    }

    [HttpDelete("rules/{ruleId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRule(
        Guid campaignId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignRulesService.DeleteRuleAsync(
            userId.Value,
            campaignId,
            ruleId,
            cancellationToken);

        return MapObjectResponse(result, "Conditional rule deleted successfully.");
    }

    [HttpGet("outcome-effects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StoryOutcomeEffectResponse>>>> GetOutcomeEffects(
        Guid campaignId,
        [FromQuery] OutcomeSourceType sourceType,
        [FromQuery] Guid sourceId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<IReadOnlyList<StoryOutcomeEffectResponse>>();
        }

        var result = await _campaignRulesService.GetOutcomeEffectsAsync(
            userId.Value,
            campaignId,
            sourceType,
            sourceId,
            cancellationToken);

        return MapListResponse(result, "Outcome effects fetched successfully.");
    }

    [HttpPost("outcome-effects")]
    public async Task<ActionResult<ApiResponse<StoryOutcomeEffectResponse>>> CreateOutcomeEffect(
        Guid campaignId,
        StoryOutcomeEffectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<StoryOutcomeEffectResponse>();
        }

        var result = await _campaignRulesService.CreateOutcomeEffectAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<StoryOutcomeEffectResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Outcome effect created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Outcome effect created successfully.")
        };
    }

    [HttpDelete("outcome-effects/{outcomeEffectId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOutcomeEffect(
        Guid campaignId,
        Guid outcomeEffectId,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<object>();
        }

        var result = await _campaignRulesService.DeleteOutcomeEffectAsync(
            userId.Value,
            campaignId,
            outcomeEffectId,
            cancellationToken);

        return MapObjectResponse(result, "Outcome effect deleted successfully.");
    }

    [HttpPost("choices")]
    public async Task<ActionResult<ApiResponse<CampaignChoiceDefinitionResponse>>> CreateChoiceDefinition(
        Guid campaignId,
        CampaignChoiceDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignChoiceDefinitionResponse>();
        }

        var result = await _campaignRulesService.CreateChoiceDefinitionAsync(
            userId.Value,
            campaignId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignChoiceDefinitionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign choice created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Campaign choice created successfully.")
        };
    }

    [HttpPost("choices/{choiceDefinitionId:guid}/options")]
    public async Task<ActionResult<ApiResponse<CampaignChoiceOptionResponse>>> CreateChoiceOption(
        Guid campaignId,
        Guid choiceDefinitionId,
        CampaignChoiceOptionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse<CampaignChoiceOptionResponse>();
        }

        var result = await _campaignRulesService.CreateChoiceOptionAsync(
            userId.Value,
            campaignId,
            choiceDefinitionId,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<CampaignChoiceOptionResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Campaign choice option created successfully.",
                Data = result.Data
            }),
            _ => MapResponse(result, "Campaign choice option created successfully.")
        };
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
            ApplicationStatusCode.InvalidCampaignEventDefinition
                or ApplicationStatusCode.InvalidCampaignEventOption
                or ApplicationStatusCode.InvalidConditionalRule
                or ApplicationStatusCode.InvalidStoryOutcomeEffect
                or ApplicationStatusCode.InvalidCampaignChoice => BadRequest(new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Campaign rule request is invalid.",
                    Data = default
                }),
            ApplicationStatusCode.UserNotFound => NotFound(new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User was not found.",
                Data = default
            }),
            ApplicationStatusCode.CampaignNotFound
                or ApplicationStatusCode.CampaignEventDefinitionNotFound
                or ApplicationStatusCode.CampaignEventOptionNotFound
                or ApplicationStatusCode.ConditionalRuleNotFound
                or ApplicationStatusCode.StoryOutcomeEffectNotFound
                or ApplicationStatusCode.CampaignChoiceNotFound
                or ApplicationStatusCode.CampaignChoiceOptionNotFound => NotFound(new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Requested campaign rule resource was not found.",
                    Data = default
                }),
            ApplicationStatusCode.CampaignEventDefinitionAlreadyExists
                or ApplicationStatusCode.CampaignEventOptionAlreadyExists
                or ApplicationStatusCode.CampaignRuleReferenceConflict
                or ApplicationStatusCode.CampaignChoiceSelectionConflict => Conflict(new ApiResponse<T>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Campaign rule request conflicts with existing campaign rule data.",
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
