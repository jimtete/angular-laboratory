using LearningLab.Data;
using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Rules;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Services.CampaignRulesService;

public interface ICampaignRulesService
{
    Task<ServiceResult<IReadOnlyList<CampaignEventDefinitionResponse>>> GetEventDefinitionsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventDefinitionResponse>> GetEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventDefinitionResponse>> CreateEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        CampaignEventDefinitionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventDefinitionResponse>> UpdateEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CampaignEventDefinitionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventOptionResponse>> CreateEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        CampaignEventOptionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventOptionResponse>> UpdateEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CampaignEventOptionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignEventStateResponse>>> GetEventStatesAsync(
        Guid userId,
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignEventStateResponse>> SetEventStateAsync(
        Guid userId,
        int campaignSessionId,
        Guid eventDefinitionId,
        CampaignEventStateRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteEventStateAsync(
        Guid userId,
        int campaignSessionId,
        Guid eventDefinitionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<ConditionalRuleResponse>>> GetRulesForTargetAsync(
        Guid userId,
        Guid campaignId,
        ConditionalTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ConditionalRuleResponse>> CreateRuleAsync(
        Guid userId,
        Guid campaignId,
        ConditionalRuleRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ConditionalRuleResponse>> UpdateRuleAsync(
        Guid userId,
        Guid campaignId,
        Guid ruleId,
        ConditionalRuleRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteRuleAsync(
        Guid userId,
        Guid campaignId,
        Guid ruleId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RuleEvaluationResult>> EvaluateRuleAsync(
        Guid userId,
        Guid conditionalRuleId,
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TargetAvailabilityResult>> IsTargetAvailableAsync(
        Guid userId,
        ConditionalTargetType targetType,
        Guid targetId,
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<TargetAvailabilityResult>>> EvaluateTargetsAsync(
        Guid userId,
        ConditionalTargetType targetType,
        IReadOnlyCollection<Guid> targetIds,
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<StoryOutcomeEffectResponse>>> GetOutcomeEffectsAsync(
        Guid userId,
        Guid campaignId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<StoryOutcomeEffectResponse>> CreateOutcomeEffectAsync(
        Guid userId,
        Guid campaignId,
        StoryOutcomeEffectRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<object>> DeleteOutcomeEffectAsync(
        Guid userId,
        Guid campaignId,
        Guid outcomeEffectId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ApplyOutcomeResult>> ApplyOutcomeEffectsAsync(
        Guid userId,
        int campaignSessionId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignChoiceDefinitionResponse>> CreateChoiceDefinitionAsync(
        Guid userId,
        Guid campaignId,
        CampaignChoiceDefinitionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignChoiceOptionResponse>> CreateChoiceOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid choiceDefinitionId,
        CampaignChoiceOptionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignChoiceSelectionResponse>>> GetChoiceSelectionsAsync(
        Guid userId,
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignChoiceSelectionResponse>> SelectChoiceOptionAsync(
        Guid userId,
        int campaignSessionId,
        SelectCampaignChoiceOptionRequest? request,
        bool allowChange = false,
        CancellationToken cancellationToken = default);
}

public sealed class CampaignRulesService : ICampaignRulesService
{
    private const int MaximumKeyLength = 128;
    private const int MaximumNameLength = 256;
    private const int MaximumDescriptionLength = 2048;

    private readonly LearningLabContext _context;

    public CampaignRulesService(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignEventDefinitionResponse>>> GetEventDefinitionsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<CampaignEventDefinitionResponse>>(validationStatusCode.Value);
        }

        var definitions = await _context.CampaignEventDefinitions
            .AsNoTracking()
            .Include(definition => definition.Options)
            .Where(definition => definition.CampaignId == campaignId)
            .OrderBy(definition => definition.Key)
            .ToListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignEventDefinitionResponse>>(
            ApplicationStatusCode.Success,
            definitions.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignEventDefinitionResponse>> GetEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(validationStatusCode.Value);
        }

        var definition = await _context.CampaignEventDefinitions
            .AsNoTracking()
            .Include(item => item.Options)
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == id,
                cancellationToken);

        return definition is null
            ? new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.CampaignEventDefinitionNotFound)
            : new ServiceResult<CampaignEventDefinitionResponse>(
                ApplicationStatusCode.Success,
                ToResponse(definition));
    }

    public async Task<ServiceResult<CampaignEventDefinitionResponse>> CreateEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        CampaignEventDefinitionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(validationStatusCode.Value);
        }

        if (!TryNormalizeEventDefinition(request, out var key, out var name, out var description))
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.InvalidCampaignEventDefinition);
        }

        var exists = await _context.CampaignEventDefinitions
            .AnyAsync(
                definition => definition.CampaignId == campaignId && definition.Key == key,
                cancellationToken);

        if (exists)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.CampaignEventDefinitionAlreadyExists);
        }

        var now = DateTimeOffset.UtcNow;
        var definition = new CampaignEventDefinition
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Key = key!,
            Name = name!,
            Description = description,
            EventType = request!.EventType,
            IsRepeatable = request.IsRepeatable,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _context.CampaignEventDefinitions.AddAsync(definition, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignEventDefinitionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(definition));
    }

    public async Task<ServiceResult<CampaignEventDefinitionResponse>> UpdateEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CampaignEventDefinitionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(validationStatusCode.Value);
        }

        if (!TryNormalizeEventDefinition(request, out var key, out var name, out var description))
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.InvalidCampaignEventDefinition);
        }

        var definition = await _context.CampaignEventDefinitions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == id,
                cancellationToken);

        if (definition is null)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.CampaignEventDefinitionNotFound);
        }

        var keyExists = await _context.CampaignEventDefinitions
            .AnyAsync(
                item => item.CampaignId == campaignId && item.Id != id && item.Key == key,
                cancellationToken);

        if (keyExists)
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.CampaignEventDefinitionAlreadyExists);
        }

        if (definition.EventType != request!.EventType
            && await _context.CampaignEventStates.AnyAsync(
                state => state.CampaignEventDefinitionId == id,
                cancellationToken))
        {
            return new ServiceResult<CampaignEventDefinitionResponse>(ApplicationStatusCode.CampaignRuleReferenceConflict);
        }

        definition.Key = key!;
        definition.Name = name!;
        definition.Description = description;
        definition.EventType = request.EventType;
        definition.IsRepeatable = request.IsRepeatable;
        definition.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignEventDefinitionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(definition));
    }

    public async Task<ServiceResult<object>> DeleteEventDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var definition = await _context.CampaignEventDefinitions
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == id,
                cancellationToken);

        if (definition is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.CampaignEventDefinitionNotFound);
        }

        var referenced = await _context.ConditionClauses
                .AnyAsync(clause => clause.CampaignEventDefinitionId == id, cancellationToken)
            || await _context.StoryOutcomeEffects
                .AnyAsync(effect => effect.CampaignEventDefinitionId == id, cancellationToken)
            || await _context.CampaignEventStates
                .AnyAsync(state => state.CampaignEventDefinitionId == id, cancellationToken);

        if (referenced)
        {
            return new ServiceResult<object>(ApplicationStatusCode.CampaignRuleReferenceConflict);
        }

        _context.CampaignEventDefinitions.Remove(definition);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<CampaignEventOptionResponse>> CreateEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        CampaignEventOptionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMutableOptionDefinitionAsync(userId, campaignId, eventDefinitionId, cancellationToken);

        if (result.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<CampaignEventOptionResponse>(result.StatusCode);
        }

        if (!TryNormalizeOption(request, out var key, out var label, out var description))
        {
            return new ServiceResult<CampaignEventOptionResponse>(ApplicationStatusCode.InvalidCampaignEventOption);
        }

        var definition = result.Data!;
        var exists = await _context.CampaignEventOptions
            .AnyAsync(
                option => option.CampaignEventDefinitionId == eventDefinitionId && option.Key == key,
                cancellationToken);

        if (exists)
        {
            return new ServiceResult<CampaignEventOptionResponse>(ApplicationStatusCode.CampaignEventOptionAlreadyExists);
        }

        var option = new CampaignEventOption
        {
            Id = Guid.NewGuid(),
            CampaignEventDefinitionId = eventDefinitionId,
            Key = key!,
            Label = label!,
            Description = description,
            SortOrder = request!.SortOrder
        };

        await _context.CampaignEventOptions.AddAsync(option, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        option.CampaignEventDefinition = definition;

        return new ServiceResult<CampaignEventOptionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(option));
    }

    public async Task<ServiceResult<CampaignEventOptionResponse>> UpdateEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CampaignEventOptionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMutableOptionDefinitionAsync(userId, campaignId, eventDefinitionId, cancellationToken);

        if (result.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<CampaignEventOptionResponse>(result.StatusCode);
        }

        if (!TryNormalizeOption(request, out var key, out var label, out var description))
        {
            return new ServiceResult<CampaignEventOptionResponse>(ApplicationStatusCode.InvalidCampaignEventOption);
        }

        var option = await _context.CampaignEventOptions
            .SingleOrDefaultAsync(
                item => item.CampaignEventDefinitionId == eventDefinitionId && item.Id == optionId,
                cancellationToken);

        if (option is null)
        {
            return new ServiceResult<CampaignEventOptionResponse>(ApplicationStatusCode.CampaignEventOptionNotFound);
        }

        var exists = await _context.CampaignEventOptions
            .AnyAsync(
                item => item.CampaignEventDefinitionId == eventDefinitionId
                    && item.Id != optionId
                    && item.Key == key,
                cancellationToken);

        if (exists)
        {
            return new ServiceResult<CampaignEventOptionResponse>(ApplicationStatusCode.CampaignEventOptionAlreadyExists);
        }

        option.Key = key!;
        option.Label = label!;
        option.Description = description;
        option.SortOrder = request!.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignEventOptionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(option));
    }

    public async Task<ServiceResult<object>> DeleteEventOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        Guid optionId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetMutableOptionDefinitionAsync(userId, campaignId, eventDefinitionId, cancellationToken);

        if (result.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<object>(result.StatusCode);
        }

        var option = await _context.CampaignEventOptions
            .SingleOrDefaultAsync(
                item => item.CampaignEventDefinitionId == eventDefinitionId && item.Id == optionId,
                cancellationToken);

        if (option is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.CampaignEventOptionNotFound);
        }

        var referenced = await _context.CampaignEventStates
                .AnyAsync(state => state.SelectedOptionId == optionId, cancellationToken)
            || await _context.ConditionClauses
                .AnyAsync(clause => clause.ExpectedOptionId == optionId, cancellationToken)
            || await _context.StoryOutcomeEffects
                .AnyAsync(effect => effect.SelectedOptionId == optionId, cancellationToken);

        if (referenced)
        {
            return new ServiceResult<object>(ApplicationStatusCode.CampaignRuleReferenceConflict);
        }

        _context.CampaignEventOptions.Remove(option);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignEventStateResponse>>> GetEventStatesAsync(
        Guid userId,
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<IReadOnlyList<CampaignEventStateResponse>>(runResult.StatusCode);
        }

        var states = await _context.CampaignEventStates
            .AsNoTracking()
            .Include(state => state.CampaignEventDefinition)
            .Include(state => state.SelectedOption)
            .Where(state => state.CampaignSessionId == campaignSessionId)
            .OrderBy(state => state.CampaignEventDefinition.Key)
            .ToListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignEventStateResponse>>(
            ApplicationStatusCode.Success,
            states.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignEventStateResponse>> SetEventStateAsync(
        Guid userId,
        int campaignSessionId,
        Guid eventDefinitionId,
        CampaignEventStateRequest? request,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<CampaignEventStateResponse>(runResult.StatusCode);
        }

        var definition = await _context.CampaignEventDefinitions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(
                item => item.CampaignId == runResult.Data!.CampaignId && item.Id == eventDefinitionId,
                cancellationToken);

        if (definition is null)
        {
            return new ServiceResult<CampaignEventStateResponse>(ApplicationStatusCode.CampaignEventDefinitionNotFound);
        }

        if (!IsValidStateValue(definition, request))
        {
            return new ServiceResult<CampaignEventStateResponse>(ApplicationStatusCode.InvalidCampaignEventState);
        }

        var state = await _context.CampaignEventStates
            .Include(item => item.CampaignEventDefinition)
            .Include(item => item.SelectedOption)
            .SingleOrDefaultAsync(
                item => item.CampaignSessionId == campaignSessionId
                    && item.CampaignEventDefinitionId == eventDefinitionId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;

        if (state is null)
        {
            state = new CampaignEventState
            {
                Id = Guid.NewGuid(),
                CampaignSessionId = campaignSessionId,
                CampaignEventDefinitionId = eventDefinitionId,
                ResolvedAtUtc = now
            };
            await _context.CampaignEventStates.AddAsync(state, cancellationToken);
        }

        ApplyStateValue(state, request!, definition.EventType);
        state.SourceStoryBlockId = request!.SourceStoryBlockId;
        state.SourceStoryBeatId = request.SourceStoryBeatId;
        state.UpdatedAtUtc = now;

        await _context.SaveChangesAsync(cancellationToken);

        state.CampaignEventDefinition = definition;
        state.SelectedOption = definition.Options.FirstOrDefault(option => option.Id == state.SelectedOptionId);

        return new ServiceResult<CampaignEventStateResponse>(
            ApplicationStatusCode.Success,
            ToResponse(state));
    }

    public async Task<ServiceResult<object>> DeleteEventStateAsync(
        Guid userId,
        int campaignSessionId,
        Guid eventDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<object>(runResult.StatusCode);
        }

        var state = await _context.CampaignEventStates
            .SingleOrDefaultAsync(
                item => item.CampaignSessionId == campaignSessionId
                    && item.CampaignEventDefinitionId == eventDefinitionId,
                cancellationToken);

        if (state is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.CampaignEventStateNotFound);
        }

        _context.CampaignEventStates.Remove(state);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<IReadOnlyList<ConditionalRuleResponse>>> GetRulesForTargetAsync(
        Guid userId,
        Guid campaignId,
        ConditionalTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<ConditionalRuleResponse>>(validationStatusCode.Value);
        }

        var rules = await _context.ConditionalRules
            .AsNoTracking()
            .Where(rule => rule.CampaignId == campaignId
                && rule.TargetType == targetType
                && rule.TargetId == targetId)
            .OrderBy(rule => rule.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var responses = new List<ConditionalRuleResponse>();

        foreach (var rule in rules)
        {
            responses.Add(await ToRuleResponseAsync(rule, cancellationToken));
        }

        return new ServiceResult<IReadOnlyList<ConditionalRuleResponse>>(
            ApplicationStatusCode.Success,
            responses);
    }

    public async Task<ServiceResult<ConditionalRuleResponse>> CreateRuleAsync(
        Guid userId,
        Guid campaignId,
        ConditionalRuleRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<ConditionalRuleResponse>(validationStatusCode.Value);
        }

        var validationResult = await ValidateRuleRequestAsync(campaignId, request, cancellationToken);

        if (validationResult != ApplicationStatusCode.Success)
        {
            return new ServiceResult<ConditionalRuleResponse>(validationResult);
        }

        var now = DateTimeOffset.UtcNow;
        var root = BuildGroup(request!.Root!, null, 1);
        var rule = new ConditionalRule
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            RootConditionGroup = root,
            RootConditionGroupId = root.Id,
            EffectType = request.EffectType,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _context.ConditionalRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<ConditionalRuleResponse>(
            ApplicationStatusCode.Success,
            await ToRuleResponseAsync(rule, cancellationToken));
    }

    public async Task<ServiceResult<ConditionalRuleResponse>> UpdateRuleAsync(
        Guid userId,
        Guid campaignId,
        Guid ruleId,
        ConditionalRuleRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<ConditionalRuleResponse>(validationStatusCode.Value);
        }

        var rule = await _context.ConditionalRules
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == ruleId,
                cancellationToken);

        if (rule is null)
        {
            return new ServiceResult<ConditionalRuleResponse>(ApplicationStatusCode.ConditionalRuleNotFound);
        }

        var validationResult = await ValidateRuleRequestAsync(campaignId, request, cancellationToken);

        if (validationResult != ApplicationStatusCode.Success)
        {
            return new ServiceResult<ConditionalRuleResponse>(validationResult);
        }

        var previousRootConditionGroupId = rule.RootConditionGroupId;
        var root = BuildGroup(request!.Root!, null, 1);
        rule.TargetType = request.TargetType;
        rule.TargetId = request.TargetId;
        rule.EffectType = request.EffectType;
        rule.RootConditionGroup = root;
        rule.RootConditionGroupId = root.Id;
        rule.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await RemoveGroupTreeAsync(previousRootConditionGroupId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ServiceResult<ConditionalRuleResponse>(
            ApplicationStatusCode.Success,
            await ToRuleResponseAsync(rule, cancellationToken));
    }

    public async Task<ServiceResult<object>> DeleteRuleAsync(
        Guid userId,
        Guid campaignId,
        Guid ruleId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var rule = await _context.ConditionalRules
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == ruleId,
                cancellationToken);

        if (rule is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.ConditionalRuleNotFound);
        }

        var rootGroupId = rule.RootConditionGroupId;
        _context.ConditionalRules.Remove(rule);
        await _context.SaveChangesAsync(cancellationToken);
        await RemoveGroupTreeAsync(rootGroupId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<RuleEvaluationResult>> EvaluateRuleAsync(
        Guid userId,
        Guid conditionalRuleId,
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        var rule = await _context.ConditionalRules
            .AsNoTracking()
            .SingleOrDefaultAsync(rule => rule.Id == conditionalRuleId, cancellationToken);

        if (rule is null)
        {
            return new ServiceResult<RuleEvaluationResult>(ApplicationStatusCode.ConditionalRuleNotFound);
        }

        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<RuleEvaluationResult>(runResult.StatusCode);
        }

        if (runResult.Data!.CampaignId != rule.CampaignId)
        {
            return new ServiceResult<RuleEvaluationResult>(ApplicationStatusCode.CampaignSessionNotFound);
        }

        return new ServiceResult<RuleEvaluationResult>(
            ApplicationStatusCode.Success,
            await EvaluateRuleCoreAsync(rule, campaignSessionId, cancellationToken));
    }

    public async Task<ServiceResult<TargetAvailabilityResult>> IsTargetAvailableAsync(
        Guid userId,
        ConditionalTargetType targetType,
        Guid targetId,
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        var results = await EvaluateTargetsAsync(
            userId,
            targetType,
            [targetId],
            campaignSessionId,
            cancellationToken);

        return results.StatusCode != ApplicationStatusCode.Success
            ? new ServiceResult<TargetAvailabilityResult>(results.StatusCode)
            : new ServiceResult<TargetAvailabilityResult>(
                ApplicationStatusCode.Success,
                results.Data!.Single());
    }

    public async Task<ServiceResult<IReadOnlyList<TargetAvailabilityResult>>> EvaluateTargetsAsync(
        Guid userId,
        ConditionalTargetType targetType,
        IReadOnlyCollection<Guid> targetIds,
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<IReadOnlyList<TargetAvailabilityResult>>(runResult.StatusCode);
        }

        if (targetIds.Count == 0)
        {
            return new ServiceResult<IReadOnlyList<TargetAvailabilityResult>>(
                ApplicationStatusCode.Success,
                []);
        }

        var targetIdSet = targetIds.ToHashSet();
        var rules = await _context.ConditionalRules
            .AsNoTracking()
            .Where(rule => rule.CampaignId == runResult.Data!.CampaignId
                && rule.TargetType == targetType
                && targetIdSet.Contains(rule.TargetId)
                && (rule.EffectType == ConditionalRuleEffectType.RequiredForAvailability
                    || rule.EffectType == ConditionalRuleEffectType.RequiredForVisibility))
            .ToListAsync(cancellationToken);

        var results = new List<TargetAvailabilityResult>();

        foreach (var targetId in targetIds)
        {
            var ruleResults = new List<RuleEvaluationResult>();

            foreach (var rule in rules.Where(rule => rule.TargetId == targetId))
            {
                ruleResults.Add(await EvaluateRuleCoreAsync(rule, campaignSessionId, cancellationToken));
            }

            results.Add(new TargetAvailabilityResult
            {
                TargetType = targetType,
                TargetId = targetId,
                IsAvailable = ruleResults.All(result => result.IsSatisfied),
                RuleResults = ruleResults
            });
        }

        return new ServiceResult<IReadOnlyList<TargetAvailabilityResult>>(
            ApplicationStatusCode.Success,
            results);
    }

    public async Task<ServiceResult<IReadOnlyList<StoryOutcomeEffectResponse>>> GetOutcomeEffectsAsync(
        Guid userId,
        Guid campaignId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryOutcomeEffectResponse>>(validationStatusCode.Value);
        }

        if (!Enum.IsDefined(sourceType)
            || !await SourceBelongsToCampaignAsync(campaignId, sourceType, sourceId, cancellationToken))
        {
            return new ServiceResult<IReadOnlyList<StoryOutcomeEffectResponse>>(ApplicationStatusCode.InvalidStoryOutcomeEffect);
        }

        var effects = await _context.StoryOutcomeEffects
            .AsNoTracking()
            .Include(effect => effect.CampaignEventDefinition)
            .Where(effect => effect.CampaignId == campaignId
                && effect.SourceType == sourceType
                && effect.SourceId == sourceId)
            .OrderBy(effect => effect.SortOrder)
            .ToListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryOutcomeEffectResponse>>(
            ApplicationStatusCode.Success,
            effects.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<StoryOutcomeEffectResponse>> CreateOutcomeEffectAsync(
        Guid userId,
        Guid campaignId,
        StoryOutcomeEffectRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryOutcomeEffectResponse>(validationStatusCode.Value);
        }

        if (request is null
            || !Enum.IsDefined(request.SourceType)
            || !Enum.IsDefined(request.OperationType)
            || !await SourceBelongsToCampaignAsync(campaignId, request.SourceType, request.SourceId, cancellationToken))
        {
            return new ServiceResult<StoryOutcomeEffectResponse>(ApplicationStatusCode.InvalidStoryOutcomeEffect);
        }

        var definition = await _context.CampaignEventDefinitions
            .Include(item => item.Options)
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == request.EventDefinitionId,
                cancellationToken);

        if (definition is null)
        {
            return new ServiceResult<StoryOutcomeEffectResponse>(ApplicationStatusCode.CampaignEventDefinitionNotFound);
        }

        if (!IsValidOutcomeValue(definition, request))
        {
            return new ServiceResult<StoryOutcomeEffectResponse>(ApplicationStatusCode.InvalidStoryOutcomeEffect);
        }

        var effect = new StoryOutcomeEffect
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            CampaignEventDefinitionId = request.EventDefinitionId,
            OperationType = request.OperationType,
            BooleanValue = request.BooleanValue,
            SelectedOptionId = request.SelectedOptionId,
            TextValue = NormalizeNullable(request.TextValue),
            NumericValue = request.NumericValue,
            SortOrder = request.SortOrder
        };

        await _context.StoryOutcomeEffects.AddAsync(effect, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        effect.CampaignEventDefinition = definition;

        return new ServiceResult<StoryOutcomeEffectResponse>(
            ApplicationStatusCode.Success,
            ToResponse(effect));
    }

    public async Task<ServiceResult<object>> DeleteOutcomeEffectAsync(
        Guid userId,
        Guid campaignId,
        Guid outcomeEffectId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(validationStatusCode.Value);
        }

        var effect = await _context.StoryOutcomeEffects
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == outcomeEffectId,
                cancellationToken);

        if (effect is null)
        {
            return new ServiceResult<object>(ApplicationStatusCode.StoryOutcomeEffectNotFound);
        }

        _context.StoryOutcomeEffects.Remove(effect);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
    }

    public async Task<ServiceResult<ApplyOutcomeResult>> ApplyOutcomeEffectsAsync(
        Guid userId,
        int campaignSessionId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<ApplyOutcomeResult>(runResult.StatusCode);
        }

        if (!await SourceBelongsToCampaignAsync(runResult.Data!.CampaignId, sourceType, sourceId, cancellationToken))
        {
            return new ServiceResult<ApplyOutcomeResult>(ApplicationStatusCode.InvalidStoryOutcomeEffect);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var changed = await ApplyOutcomeEffectsCoreAsync(
            runResult.Data,
            sourceType,
            sourceId,
            cancellationToken);

        if (changed.StatusCode != ApplicationStatusCode.Success)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new ServiceResult<ApplyOutcomeResult>(changed.StatusCode);
        }

        await transaction.CommitAsync(cancellationToken);

        return new ServiceResult<ApplyOutcomeResult>(
            ApplicationStatusCode.Success,
            new ApplyOutcomeResult
            {
                ChangedEventStates = changed.Data!
            });
    }

    public async Task<ServiceResult<CampaignChoiceDefinitionResponse>> CreateChoiceDefinitionAsync(
        Guid userId,
        Guid campaignId,
        CampaignChoiceDefinitionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignChoiceDefinitionResponse>(validationStatusCode.Value);
        }

        var name = request?.Name?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(name)
            || name.Length > MaximumNameLength
            || !Enum.IsDefined(request.SelectionMode)
            || (request.StoryBlockId is null && request.StoryBeatId is null)
            || request.StoryBlockId is not null && !await StoryBlockBelongsToCampaignAsync(campaignId, request.StoryBlockId.Value, cancellationToken)
            || request.StoryBeatId is not null && !await StoryBeatBelongsToCampaignAsync(campaignId, request.StoryBeatId.Value, cancellationToken))
        {
            return new ServiceResult<CampaignChoiceDefinitionResponse>(ApplicationStatusCode.InvalidCampaignChoice);
        }

        var choice = new CampaignChoiceDefinition
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            StoryBlockId = request.StoryBlockId,
            StoryBeatId = request.StoryBeatId,
            Name = name,
            SelectionMode = request.SelectionMode
        };

        await _context.CampaignChoiceDefinitions.AddAsync(choice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignChoiceDefinitionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(choice));
    }

    public async Task<ServiceResult<CampaignChoiceOptionResponse>> CreateChoiceOptionAsync(
        Guid userId,
        Guid campaignId,
        Guid choiceDefinitionId,
        CampaignChoiceOptionRequest? request,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignChoiceOptionResponse>(validationStatusCode.Value);
        }

        var choice = await _context.CampaignChoiceDefinitions
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == choiceDefinitionId,
                cancellationToken);

        if (choice is null)
        {
            return new ServiceResult<CampaignChoiceOptionResponse>(ApplicationStatusCode.CampaignChoiceNotFound);
        }

        if (!TryNormalizeOption(
                new CampaignEventOptionRequest
                {
                    Key = request?.Key,
                    Label = request?.Label,
                    Description = request?.Description,
                    SortOrder = request?.SortOrder ?? 0
                },
                out var key,
                out var label,
                out var description)
            || request?.StoryBeatId is not null
                && !await StoryBeatBelongsToCampaignAsync(campaignId, request.StoryBeatId.Value, cancellationToken))
        {
            return new ServiceResult<CampaignChoiceOptionResponse>(ApplicationStatusCode.InvalidCampaignChoice);
        }

        var exists = await _context.CampaignChoiceOptions
            .AnyAsync(
                item => item.CampaignChoiceDefinitionId == choiceDefinitionId
                    && item.Key == key,
                cancellationToken);

        if (exists)
        {
            return new ServiceResult<CampaignChoiceOptionResponse>(ApplicationStatusCode.CampaignChoiceSelectionConflict);
        }

        var option = new CampaignChoiceOption
        {
            Id = Guid.NewGuid(),
            CampaignChoiceDefinitionId = choiceDefinitionId,
            StoryBeatId = request!.StoryBeatId,
            Key = key!,
            Label = label!,
            Description = description,
            SortOrder = request.SortOrder
        };

        await _context.CampaignChoiceOptions.AddAsync(option, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignChoiceOptionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(option));
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignChoiceSelectionResponse>>> GetChoiceSelectionsAsync(
        Guid userId,
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<IReadOnlyList<CampaignChoiceSelectionResponse>>(runResult.StatusCode);
        }

        var selections = await _context.CampaignChoiceSelections
            .AsNoTracking()
            .Where(selection => selection.CampaignSessionId == campaignSessionId)
            .OrderBy(selection => selection.SelectedAtUtc)
            .ToListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignChoiceSelectionResponse>>(
            ApplicationStatusCode.Success,
            selections.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignChoiceSelectionResponse>> SelectChoiceOptionAsync(
        Guid userId,
        int campaignSessionId,
        SelectCampaignChoiceOptionRequest? request,
        bool allowChange = false,
        CancellationToken cancellationToken = default)
    {
        var runResult = await GetSessionForUserAsync(userId, campaignSessionId, cancellationToken);

        if (runResult.StatusCode != ApplicationStatusCode.Success)
        {
            return new ServiceResult<CampaignChoiceSelectionResponse>(runResult.StatusCode);
        }

        var option = await _context.CampaignChoiceOptions
            .Include(item => item.CampaignChoiceDefinition)
            .SingleOrDefaultAsync(
                item => item.Id == request!.ChoiceOptionId,
                cancellationToken);

        if (request is null || option is null)
        {
            return new ServiceResult<CampaignChoiceSelectionResponse>(ApplicationStatusCode.CampaignChoiceOptionNotFound);
        }

        if (option.CampaignChoiceDefinition.CampaignId != runResult.Data!.CampaignId)
        {
            return new ServiceResult<CampaignChoiceSelectionResponse>(ApplicationStatusCode.CampaignChoiceOptionNotFound);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var existingSelections = await _context.CampaignChoiceSelections
            .Where(selection => selection.CampaignSessionId == campaignSessionId
                && selection.CampaignChoiceDefinitionId == option.CampaignChoiceDefinitionId)
            .ToListAsync(cancellationToken);

        var exclusive = option.CampaignChoiceDefinition.SelectionMode is CampaignChoiceSelectionMode.Single
            or CampaignChoiceSelectionMode.ExactlyOne;

        if (existingSelections.Count > 0 && exclusive && !allowChange)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new ServiceResult<CampaignChoiceSelectionResponse>(ApplicationStatusCode.CampaignChoiceSelectionConflict);
        }

        if (existingSelections.Any(selection => selection.CampaignChoiceOptionId == option.Id))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new ServiceResult<CampaignChoiceSelectionResponse>(ApplicationStatusCode.CampaignChoiceSelectionConflict);
        }

        if (exclusive && existingSelections.Count > 0)
        {
            _context.CampaignChoiceSelections.RemoveRange(existingSelections);
        }

        var selection = new CampaignChoiceSelection
        {
            Id = Guid.NewGuid(),
            CampaignSessionId = campaignSessionId,
            CampaignChoiceDefinitionId = option.CampaignChoiceDefinitionId,
            CampaignChoiceOptionId = option.Id,
            SelectedAtUtc = DateTimeOffset.UtcNow
        };

        await _context.CampaignChoiceSelections.AddAsync(selection, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var applyResult = await ApplyOutcomeEffectsCoreAsync(
            runResult.Data,
            OutcomeSourceType.ChoiceOption,
            option.Id,
            cancellationToken);

        if (applyResult.StatusCode != ApplicationStatusCode.Success)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new ServiceResult<CampaignChoiceSelectionResponse>(applyResult.StatusCode);
        }

        await transaction.CommitAsync(cancellationToken);

        return new ServiceResult<CampaignChoiceSelectionResponse>(
            ApplicationStatusCode.Success,
            ToResponse(selection));
    }

    private async Task<ServiceResult<IReadOnlyList<CampaignEventStateResponse>>> ApplyOutcomeEffectsCoreAsync(
        CampaignSession session,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var effects = await _context.StoryOutcomeEffects
            .Include(effect => effect.CampaignEventDefinition)
                .ThenInclude(definition => definition.Options)
            .Where(effect => effect.CampaignId == session.CampaignId
                && effect.SourceType == sourceType
                && effect.SourceId == sourceId)
            .OrderBy(effect => effect.SortOrder)
            .ToListAsync(cancellationToken);

        foreach (var effect in effects)
        {
            if (!IsValidOutcomeValue(effect.CampaignEventDefinition, effect))
            {
                return new ServiceResult<IReadOnlyList<CampaignEventStateResponse>>(ApplicationStatusCode.InvalidStoryOutcomeEffect);
            }
        }

        var eventDefinitionIds = effects
            .Select(effect => effect.CampaignEventDefinitionId)
            .Distinct()
            .ToList();

        var states = await _context.CampaignEventStates
            .Include(state => state.CampaignEventDefinition)
            .Include(state => state.SelectedOption)
            .Where(state => state.CampaignSessionId == session.Id
                && eventDefinitionIds.Contains(state.CampaignEventDefinitionId))
            .ToListAsync(cancellationToken);

        var changedStates = new List<CampaignEventState>();
        var now = DateTimeOffset.UtcNow;
        var sourceStoryBeatId = sourceType == OutcomeSourceType.StoryBeat
            ? sourceId
            : await GetNestedStoryBeatIdAsync(
                session.CampaignId,
                sourceType,
                sourceId,
                cancellationToken);

        foreach (var effect in effects)
        {
            var state = states.SingleOrDefault(item => item.CampaignEventDefinitionId == effect.CampaignEventDefinitionId);

            if (effect.OperationType == OutcomeOperationType.Clear)
            {
                if (state is not null)
                {
                    _context.CampaignEventStates.Remove(state);
                    states.Remove(state);
                }

                continue;
            }

            if (state is null)
            {
                state = new CampaignEventState
                {
                    Id = Guid.NewGuid(),
                    CampaignSessionId = session.Id,
                    CampaignEventDefinitionId = effect.CampaignEventDefinitionId,
                    CampaignEventDefinition = effect.CampaignEventDefinition,
                    ResolvedAtUtc = now
                };
                states.Add(state);
                await _context.CampaignEventStates.AddAsync(state, cancellationToken);
            }

            ApplyEffectToState(
                state,
                effect,
                sourceType,
                sourceId,
                sourceStoryBeatId);
            state.UpdatedAtUtc = now;
            changedStates.Add(state);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignEventStateResponse>>(
            ApplicationStatusCode.Success,
            changedStates.Select(ToResponse).ToList());
    }

    private async Task<RuleEvaluationResult> EvaluateRuleCoreAsync(
        ConditionalRule rule,
        int campaignSessionId,
        CancellationToken cancellationToken)
    {
        var groupTree = await LoadGroupTreeAsync(rule.RootConditionGroupId, cancellationToken);
        var eventDefinitionIds = groupTree
            .Groups
            .SelectMany(group => group.Clauses)
            .Select(clause => clause.CampaignEventDefinitionId)
            .Distinct()
            .ToList();

        var states = await _context.CampaignEventStates
            .AsNoTracking()
            .Include(state => state.CampaignEventDefinition)
            .Include(state => state.SelectedOption)
            .Where(state => state.CampaignSessionId == campaignSessionId
                && eventDefinitionIds.Contains(state.CampaignEventDefinitionId))
            .ToDictionaryAsync(
                state => state.CampaignEventDefinitionId,
                cancellationToken);

        var definitions = await _context.CampaignEventDefinitions
            .AsNoTracking()
            .Where(definition => eventDefinitionIds.Contains(definition.Id))
            .ToDictionaryAsync(definition => definition.Id, cancellationToken);

        var evaluatedGroup = EvaluateGroup(groupTree.Root, groupTree.ChildrenByParentId, states, definitions);
        var failedClauses = FlattenFailedClauses(evaluatedGroup).ToList();
        var missingEvents = failedClauses
            .Where(clause => clause.Explanation.Contains("is not set", StringComparison.OrdinalIgnoreCase))
            .Select(clause => new MissingEventResponse
            {
                EventDefinitionId = clause.EventDefinitionId,
                EventKey = clause.EventKey
            })
            .DistinctBy(item => item.EventDefinitionId)
            .ToList();

        return new RuleEvaluationResult
        {
            RuleId = rule.Id,
            IsSatisfied = evaluatedGroup.IsSatisfied,
            EvaluatedGroup = evaluatedGroup,
            FailedClauses = failedClauses,
            MissingEvents = missingEvents,
            HumanReadableExplanation = evaluatedGroup.IsSatisfied
                ? "Available because all required conditions are satisfied."
                : $"Unavailable because {failedClauses.FirstOrDefault()?.Explanation ?? "one or more conditions failed"}."
        };
    }

    private static EvaluatedConditionGroupResponse EvaluateGroup(
        ConditionGroup group,
        IReadOnlyDictionary<Guid, List<ConditionGroup>> childrenByParentId,
        IReadOnlyDictionary<Guid, CampaignEventState> states,
        IReadOnlyDictionary<Guid, CampaignEventDefinition> definitions)
    {
        var clauses = group.Clauses
            .OrderBy(clause => clause.SortOrder)
            .Select(clause => EvaluateClause(clause, states, definitions))
            .ToList();
        var groups = childrenByParentId.TryGetValue(group.Id, out var children)
            ? children
                .OrderBy(child => child.SortOrder)
                .Select(child => EvaluateGroup(child, childrenByParentId, states, definitions))
                .ToList()
            : [];
        var itemResults = clauses
            .Select(clause => clause.IsSatisfied)
            .Concat(groups.Select(child => child.IsSatisfied))
            .ToList();
        var isSatisfied = group.Operator switch
        {
            ConditionGroupOperator.And => itemResults.All(BooleanIdentity),
            ConditionGroupOperator.Or => itemResults.Any(BooleanIdentity),
            ConditionGroupOperator.ExactlyOne => itemResults.Count(BooleanIdentity) == 1,
            _ => false
        };

        if (group.Negate)
        {
            isSatisfied = !isSatisfied;
        }

        return new EvaluatedConditionGroupResponse
        {
            GroupId = group.Id,
            Operator = group.Operator,
            Negate = group.Negate,
            IsSatisfied = isSatisfied,
            Clauses = clauses,
            Groups = groups
        };

        static bool BooleanIdentity(bool value) => value;
    }

    private static EvaluatedConditionClauseResponse EvaluateClause(
        ConditionClause clause,
        IReadOnlyDictionary<Guid, CampaignEventState> states,
        IReadOnlyDictionary<Guid, CampaignEventDefinition> definitions)
    {
        var definition = definitions[clause.CampaignEventDefinitionId];
        states.TryGetValue(clause.CampaignEventDefinitionId, out var state);
        var isSet = StateHasValue(state, definition.EventType);
        var isSatisfied = clause.ComparisonOperator switch
        {
            ConditionComparisonOperator.IsSet => isSet,
            ConditionComparisonOperator.IsNotSet => !isSet,
            ConditionComparisonOperator.Equals => isSet && CompareState(state!, clause, definition.EventType) == 0,
            ConditionComparisonOperator.NotEquals => !isSet || CompareState(state!, clause, definition.EventType) != 0,
            ConditionComparisonOperator.GreaterThan => isSet && CompareState(state!, clause, definition.EventType) > 0,
            ConditionComparisonOperator.GreaterThanOrEqual => isSet && CompareState(state!, clause, definition.EventType) >= 0,
            ConditionComparisonOperator.LessThan => isSet && CompareState(state!, clause, definition.EventType) < 0,
            ConditionComparisonOperator.LessThanOrEqual => isSet && CompareState(state!, clause, definition.EventType) <= 0,
            _ => false
        };

        return new EvaluatedConditionClauseResponse
        {
            ClauseId = clause.Id,
            EventDefinitionId = definition.Id,
            EventKey = definition.Key,
            ComparisonOperator = clause.ComparisonOperator,
            IsSatisfied = isSatisfied,
            Explanation = isSatisfied
                ? $"{definition.Key} satisfied {clause.ComparisonOperator}."
                : BuildFailureExplanation(definition, clause, state)
        };
    }

    private async Task<(ConditionGroup Root, List<ConditionGroup> Groups, Dictionary<Guid, List<ConditionGroup>> ChildrenByParentId)>
        LoadGroupTreeAsync(Guid rootGroupId, CancellationToken cancellationToken)
    {
        var groups = new List<ConditionGroup>();
        var currentIds = new List<Guid> { rootGroupId };

        while (currentIds.Count > 0)
        {
            var levelGroups = await _context.ConditionGroups
                .AsNoTracking()
                .Include(group => group.Clauses)
                    .ThenInclude(clause => clause.CampaignEventDefinition)
                .Where(group => currentIds.Contains(group.Id))
                .ToListAsync(cancellationToken);

            groups.AddRange(levelGroups);
            currentIds = await _context.ConditionGroups
                .AsNoTracking()
                .Where(group => group.ParentConditionGroupId.HasValue
                    && currentIds.Contains(group.ParentConditionGroupId.Value))
                .Select(group => group.Id)
                .ToListAsync(cancellationToken);
        }

        return (
            groups.Single(group => group.Id == rootGroupId),
            groups,
            groups.GroupBy(group => group.ParentConditionGroupId)
                .ToDictionary(group => group.Key ?? Guid.Empty, group => group.ToList()));
    }

    private async Task<ConditionalRuleResponse> ToRuleResponseAsync(
        ConditionalRule rule,
        CancellationToken cancellationToken)
    {
        var tree = await LoadGroupTreeAsync(rule.RootConditionGroupId, cancellationToken);

        return new ConditionalRuleResponse
        {
            Id = rule.Id,
            CampaignId = rule.CampaignId,
            TargetType = rule.TargetType,
            TargetId = rule.TargetId,
            EffectType = rule.EffectType,
            Root = ToResponse(tree.Root, tree.ChildrenByParentId),
            CreatedAtUtc = rule.CreatedAtUtc,
            UpdatedAtUtc = rule.UpdatedAtUtc
        };
    }

    private async Task<ApplicationStatusCode> ValidateRuleRequestAsync(
        Guid campaignId,
        ConditionalRuleRequest? request,
        CancellationToken cancellationToken)
    {
        if (request?.Root is null
            || !Enum.IsDefined(request.TargetType)
            || !Enum.IsDefined(request.EffectType)
            || !await TargetBelongsToCampaignAsync(campaignId, request.TargetType, request.TargetId, cancellationToken)
            || !IsValidGroupShape(request.Root, 0))
        {
            return ApplicationStatusCode.InvalidConditionalRule;
        }

        var eventDefinitionIds = new List<Guid>();
        var optionIds = new List<Guid>();
        CollectConditionReferences(request.Root, eventDefinitionIds, optionIds);

        var definitions = await _context.CampaignEventDefinitions
            .Include(definition => definition.Options)
            .Where(definition => eventDefinitionIds.Contains(definition.Id))
            .ToDictionaryAsync(definition => definition.Id, cancellationToken);

        if (definitions.Count != eventDefinitionIds.Distinct().Count()
            || definitions.Values.Any(definition => definition.CampaignId != campaignId))
        {
            return ApplicationStatusCode.InvalidConditionalRule;
        }

        if (!ValidateClauses(request.Root, definitions))
        {
            return ApplicationStatusCode.InvalidConditionalRule;
        }

        return ApplicationStatusCode.Success;
    }

    private async Task RemoveGroupTreeAsync(Guid rootGroupId, CancellationToken cancellationToken)
    {
        var tree = await LoadGroupTreeAsync(rootGroupId, cancellationToken);
        var groupIds = tree.Groups
            .Select(group => group.Id)
            .ToList();

        await _context.ConditionClauses
            .Where(clause => groupIds.Contains(clause.ConditionGroupId))
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var groupId in groupIds.AsEnumerable().Reverse())
        {
            await _context.ConditionGroups
                .Where(group => group.Id == groupId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    private static ConditionGroup BuildGroup(
        ConditionGroupRequest request,
        Guid? parentConditionGroupId,
        int sortOrder)
    {
        var group = new ConditionGroup
        {
            Id = Guid.NewGuid(),
            ParentConditionGroupId = parentConditionGroupId,
            Operator = request.Operator,
            Negate = request.Negate,
            SortOrder = sortOrder
        };

        group.Clauses = request.Clauses
            .Select((clause, index) => new ConditionClause
            {
                Id = Guid.NewGuid(),
                ConditionGroupId = group.Id,
                CampaignEventDefinitionId = clause.EventDefinitionId,
                ComparisonOperator = clause.ComparisonOperator,
                BooleanValue = clause.BooleanValue,
                ExpectedOptionId = clause.ExpectedOptionId,
                TextValue = NormalizeNullable(clause.TextValue),
                NumericValue = clause.NumericValue,
                SortOrder = index + 1
            })
            .ToList();

        group.Groups = request.Groups
            .Select((child, index) => BuildGroup(child, group.Id, index + 1))
            .ToList();

        return group;
    }

    private async Task<ApplicationStatusCode?> ValidateCampaignAccessAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);

        if (user is null)
        {
            return ApplicationStatusCode.UserNotFound;
        }

        if (!user.UserRoles.Any(userRole => string.Equals(
                userRole.Role.Name,
                AccessRoleNames.Master,
                StringComparison.OrdinalIgnoreCase)))
        {
            return ApplicationStatusCode.CampaignMasterRoleRequired;
        }

        var campaignExists = await _context.Campaigns
            .AsNoTracking()
            .AnyAsync(
                campaign => campaign.CampaignId == campaignId && campaign.GameMasterId == userId,
                cancellationToken);

        return campaignExists ? null : ApplicationStatusCode.CampaignNotFound;
    }

    private async Task<ServiceResult<CampaignSession>> GetSessionForUserAsync(
        Guid userId,
        int campaignSessionId,
        CancellationToken cancellationToken)
    {
        var run = await _context.CampaignSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == campaignSessionId, cancellationToken);

        if (run is null)
        {
            return new ServiceResult<CampaignSession>(ApplicationStatusCode.CampaignSessionNotFound);
        }

        var validationStatusCode = await ValidateCampaignAccessAsync(userId, run.CampaignId, cancellationToken);

        return validationStatusCode is null
            ? new ServiceResult<CampaignSession>(ApplicationStatusCode.Success, run)
            : new ServiceResult<CampaignSession>(validationStatusCode.Value);
    }

    private async Task<ServiceResult<CampaignEventDefinition>> GetMutableOptionDefinitionAsync(
        Guid userId,
        Guid campaignId,
        Guid eventDefinitionId,
        CancellationToken cancellationToken)
    {
        var validationStatusCode = await ValidateCampaignAccessAsync(userId, campaignId, cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignEventDefinition>(validationStatusCode.Value);
        }

        var definition = await _context.CampaignEventDefinitions
            .SingleOrDefaultAsync(
                item => item.CampaignId == campaignId && item.Id == eventDefinitionId,
                cancellationToken);

        if (definition is null)
        {
            return new ServiceResult<CampaignEventDefinition>(ApplicationStatusCode.CampaignEventDefinitionNotFound);
        }

        return definition.EventType != CampaignEventType.SingleChoice
            ? new ServiceResult<CampaignEventDefinition>(ApplicationStatusCode.InvalidCampaignEventOption)
            : new ServiceResult<CampaignEventDefinition>(ApplicationStatusCode.Success, definition);
    }

    private async Task<bool> TargetBelongsToCampaignAsync(
        Guid campaignId,
        ConditionalTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        return targetType switch
        {
            ConditionalTargetType.StoryBlock => await StoryBlockBelongsToCampaignAsync(campaignId, targetId, cancellationToken),
            ConditionalTargetType.StoryBeat => await StoryBeatBelongsToCampaignAsync(campaignId, targetId, cancellationToken),
            ConditionalTargetType.InformationBeat => await InformationBeatBelongsToCampaignAsync(campaignId, targetId, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> SourceBelongsToCampaignAsync(
        Guid campaignId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        return sourceType switch
        {
            OutcomeSourceType.StoryBlock => await StoryBlockBelongsToCampaignAsync(campaignId, sourceId, cancellationToken),
            OutcomeSourceType.StoryBeat => await StoryBeatBelongsToCampaignAsync(campaignId, sourceId, cancellationToken),
            OutcomeSourceType.ChoiceOption => await _context.CampaignChoiceOptions
                .AsNoTracking()
                .Include(option => option.CampaignChoiceDefinition)
                .AnyAsync(
                    option => option.Id == sourceId
                        && option.CampaignChoiceDefinition.CampaignId == campaignId,
                    cancellationToken),
            OutcomeSourceType.DecisionChoice => await GetNestedStoryBeatIdAsync(
                    campaignId,
                    sourceType,
                    sourceId,
                    cancellationToken)
                is not null,
            OutcomeSourceType.RoleplayingNpcInteraction => await GetRoleplayingSourceStoryBeatIdAsync(
                    campaignId,
                    sourceType,
                    sourceId,
                    cancellationToken)
                is not null,
            OutcomeSourceType.RoleplayingInformation => await GetRoleplayingSourceStoryBeatIdAsync(
                    campaignId,
                    sourceType,
                    sourceId,
                    cancellationToken)
                is not null,
            _ => false
        };
    }

    private async Task<Guid?> GetRoleplayingSourceStoryBeatIdAsync(
        Guid campaignId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        if (sourceType is not OutcomeSourceType.RoleplayingNpcInteraction
            and not OutcomeSourceType.RoleplayingInformation)
        {
            return null;
        }

        var storyBeats = await _context.StoryBeats
            .AsNoTracking()
            .Include(beat => beat.StoryBlock)
            .Where(beat => beat.StoryBlock.CampaignId == campaignId
                && beat.StoryBeatType == StoryBeatType.Roleplaying)
            .ToListAsync(cancellationToken);

        return storyBeats
            .FirstOrDefault(beat => sourceType switch
            {
                OutcomeSourceType.RoleplayingNpcInteraction => beat.Roleplaying?.NpcReferences
                    .Any(reference => reference.Id == sourceId) == true,
                OutcomeSourceType.RoleplayingInformation => beat.Roleplaying?.DiscoverableInformation
                    .Any(information => information.Id == sourceId) == true,
                _ => false
            })
            ?.Id;
    }

    private async Task<Guid?> GetNestedStoryBeatIdAsync(
        Guid campaignId,
        OutcomeSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        if (sourceType is OutcomeSourceType.RoleplayingNpcInteraction
            or OutcomeSourceType.RoleplayingInformation)
        {
            return await GetRoleplayingSourceStoryBeatIdAsync(
                campaignId,
                sourceType,
                sourceId,
                cancellationToken);
        }

        if (sourceType is not OutcomeSourceType.DecisionChoice)
        {
            return null;
        }

        var storyBeats = await _context.StoryBeats
            .AsNoTracking()
            .Include(beat => beat.StoryBlock)
            .Where(beat => beat.StoryBlock.CampaignId == campaignId
                && beat.StoryBeatType == StoryBeatType.Decision)
            .ToListAsync(cancellationToken);

        return storyBeats
            .FirstOrDefault(beat => beat.Decision?.Decisions
                .Any(decision => decision.Id == sourceId
                    || (decision.Id == Guid.Empty
                        && CreateDeterministicDecisionOptionId(beat.Id, decision.OrderIndex) == sourceId)) == true)
            ?.Id;
    }

    private static Guid CreateDeterministicDecisionOptionId(
        Guid storyBeatId,
        int orderIndex)
    {
        var input = $"{storyBeatId:N}:decision:{orderIndex}";
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, guidBytes.Length);

        return new Guid(guidBytes);
    }

    private Task<bool> StoryBlockBelongsToCampaignAsync(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken)
    {
        return _context.StoryBlocks
            .AsNoTracking()
            .AnyAsync(
                block => block.CampaignId == campaignId && block.StoryBlockId == storyBlockId,
                cancellationToken);
    }

    private Task<bool> StoryBeatBelongsToCampaignAsync(
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken)
    {
        return _context.StoryBeats
            .AsNoTracking()
            .Include(beat => beat.StoryBlock)
            .AnyAsync(
                beat => beat.Id == storyBeatId && beat.StoryBlock.CampaignId == campaignId,
                cancellationToken);
    }

    private async Task<bool> InformationBeatBelongsToCampaignAsync(
        Guid campaignId,
        Guid informationBeatId,
        CancellationToken cancellationToken)
    {
        var beats = await _context.StoryBeats
            .AsNoTracking()
            .Include(beat => beat.StoryBlock)
            .Where(beat => beat.StoryBlock.CampaignId == campaignId
                && beat.StoryBeatType == StoryBeatType.Information)
            .ToListAsync(cancellationToken);

        return beats.Any(beat => beat.Information?.OptionalInformation
            .Any(information => information.Id == informationBeatId) == true);
    }

    private static bool TryNormalizeEventDefinition(
        CampaignEventDefinitionRequest? request,
        out string? key,
        out string? name,
        out string? description)
    {
        key = NormalizeKey(request?.Key);
        name = request?.Name?.Trim();
        description = NormalizeNullable(request?.Description);

        return request is not null
            && !string.IsNullOrWhiteSpace(key)
            && key.Length <= MaximumKeyLength
            && !string.IsNullOrWhiteSpace(name)
            && name.Length <= MaximumNameLength
            && description?.Length <= MaximumDescriptionLength
            && Enum.IsDefined(request.EventType);
    }

    private static bool TryNormalizeOption(
        CampaignEventOptionRequest? request,
        out string? key,
        out string? label,
        out string? description)
    {
        key = NormalizeKey(request?.Key);
        label = request?.Label?.Trim();
        description = NormalizeNullable(request?.Description);

        return request is not null
            && !string.IsNullOrWhiteSpace(key)
            && key.Length <= MaximumKeyLength
            && !string.IsNullOrWhiteSpace(label)
            && label.Length <= MaximumNameLength
            && description?.Length <= MaximumDescriptionLength;
    }

    private static bool IsValidGroupShape(ConditionGroupRequest group, int depth)
    {
        return depth <= 16
            && Enum.IsDefined(group.Operator)
            && group.Clauses.Count + group.Groups.Count > 0
            && group.Clauses.All(clause => Enum.IsDefined(clause.ComparisonOperator))
            && group.Groups.All(child => IsValidGroupShape(child, depth + 1));
    }

    private static void CollectConditionReferences(
        ConditionGroupRequest group,
        ICollection<Guid> eventDefinitionIds,
        ICollection<Guid> optionIds)
    {
        foreach (var clause in group.Clauses)
        {
            eventDefinitionIds.Add(clause.EventDefinitionId);

            if (clause.ExpectedOptionId is not null)
            {
                optionIds.Add(clause.ExpectedOptionId.Value);
            }
        }

        foreach (var child in group.Groups)
        {
            CollectConditionReferences(child, eventDefinitionIds, optionIds);
        }
    }

    private static bool ValidateClauses(
        ConditionGroupRequest group,
        IReadOnlyDictionary<Guid, CampaignEventDefinition> definitions)
    {
        return group.Clauses.All(clause => ValidateClause(clause, definitions[clause.EventDefinitionId]))
            && group.Groups.All(child => ValidateClauses(child, definitions));
    }

    private static bool ValidateClause(
        ConditionClauseRequest clause,
        CampaignEventDefinition definition)
    {
        if (clause.ComparisonOperator is ConditionComparisonOperator.IsSet
            or ConditionComparisonOperator.IsNotSet)
        {
            return clause.BooleanValue is null
                && clause.ExpectedOptionId is null
                && clause.TextValue is null
                && clause.NumericValue is null;
        }

        if (clause.ComparisonOperator is ConditionComparisonOperator.GreaterThan
                or ConditionComparisonOperator.GreaterThanOrEqual
                or ConditionComparisonOperator.LessThan
                or ConditionComparisonOperator.LessThanOrEqual
            && definition.EventType != CampaignEventType.NumericValue)
        {
            return false;
        }

        return definition.EventType switch
        {
            CampaignEventType.BooleanFlag => clause.BooleanValue is not null
                && clause.ExpectedOptionId is null
                && clause.TextValue is null
                && clause.NumericValue is null,
            CampaignEventType.SingleChoice => clause.BooleanValue is null
                && clause.ExpectedOptionId is not null
                && clause.TextValue is null
                && clause.NumericValue is null
                && definition.Options.Any(option => option.Id == clause.ExpectedOptionId),
            CampaignEventType.TextValue => clause.BooleanValue is null
                && clause.ExpectedOptionId is null
                && !string.IsNullOrWhiteSpace(clause.TextValue)
                && clause.NumericValue is null,
            CampaignEventType.NumericValue => clause.BooleanValue is null
                && clause.ExpectedOptionId is null
                && clause.TextValue is null
                && clause.NumericValue is not null,
            _ => false
        };
    }

    private static bool IsValidStateValue(
        CampaignEventDefinition definition,
        CampaignEventStateRequest? request)
    {
        return request is not null
            && definition.EventType switch
            {
                CampaignEventType.BooleanFlag => request.BooleanValue is not null
                    && request.SelectedOptionId is null
                    && request.TextValue is null
                    && request.NumericValue is null,
                CampaignEventType.SingleChoice => request.BooleanValue is null
                    && request.SelectedOptionId is not null
                    && request.TextValue is null
                    && request.NumericValue is null
                    && definition.Options.Any(option => option.Id == request.SelectedOptionId),
                CampaignEventType.TextValue => request.BooleanValue is null
                    && request.SelectedOptionId is null
                    && !string.IsNullOrWhiteSpace(request.TextValue)
                    && request.NumericValue is null,
                CampaignEventType.NumericValue => request.BooleanValue is null
                    && request.SelectedOptionId is null
                    && request.TextValue is null
                    && request.NumericValue is not null,
                _ => false
            };
    }

    private static bool IsValidOutcomeValue(
        CampaignEventDefinition definition,
        StoryOutcomeEffectRequest request)
    {
        if (request.OperationType == OutcomeOperationType.Clear)
        {
            return request.BooleanValue is null
                && request.SelectedOptionId is null
                && request.TextValue is null
                && request.NumericValue is null;
        }

        if (request.OperationType is OutcomeOperationType.Increment or OutcomeOperationType.Decrement)
        {
            return definition.EventType == CampaignEventType.NumericValue
                && request.NumericValue is not null
                && request.BooleanValue is null
                && request.SelectedOptionId is null
                && request.TextValue is null;
        }

        return IsValidEffectSetValue(
            definition,
            request.BooleanValue,
            request.SelectedOptionId,
            request.TextValue,
            request.NumericValue);
    }

    private static bool IsValidOutcomeValue(
        CampaignEventDefinition definition,
        StoryOutcomeEffect effect)
    {
        if (effect.OperationType == OutcomeOperationType.Clear)
        {
            return true;
        }

        if (effect.OperationType is OutcomeOperationType.Increment or OutcomeOperationType.Decrement)
        {
            return definition.EventType == CampaignEventType.NumericValue
                && effect.NumericValue is not null;
        }

        return IsValidEffectSetValue(
            definition,
            effect.BooleanValue,
            effect.SelectedOptionId,
            effect.TextValue,
            effect.NumericValue);
    }

    private static bool IsValidEffectSetValue(
        CampaignEventDefinition definition,
        bool? booleanValue,
        Guid? selectedOptionId,
        string? textValue,
        decimal? numericValue)
    {
        return definition.EventType switch
        {
            CampaignEventType.BooleanFlag => booleanValue is not null
                && selectedOptionId is null
                && textValue is null
                && numericValue is null,
            CampaignEventType.SingleChoice => booleanValue is null
                && selectedOptionId is not null
                && textValue is null
                && numericValue is null
                && definition.Options.Any(option => option.Id == selectedOptionId),
            CampaignEventType.TextValue => booleanValue is null
                && selectedOptionId is null
                && !string.IsNullOrWhiteSpace(textValue)
                && numericValue is null,
            CampaignEventType.NumericValue => booleanValue is null
                && selectedOptionId is null
                && textValue is null
                && numericValue is not null,
            _ => false
        };
    }

    private static void ApplyStateValue(
        CampaignEventState state,
        CampaignEventStateRequest request,
        CampaignEventType eventType)
    {
        state.BooleanValue = eventType == CampaignEventType.BooleanFlag ? request.BooleanValue : null;
        state.SelectedOptionId = eventType == CampaignEventType.SingleChoice ? request.SelectedOptionId : null;
        state.TextValue = eventType == CampaignEventType.TextValue ? NormalizeNullable(request.TextValue) : null;
        state.NumericValue = eventType == CampaignEventType.NumericValue ? request.NumericValue : null;
    }

    private static void ApplyEffectToState(
        CampaignEventState state,
        StoryOutcomeEffect effect,
        OutcomeSourceType sourceType,
        Guid sourceId,
        Guid? sourceStoryBeatId)
    {
        if (effect.OperationType == OutcomeOperationType.Set)
        {
            state.BooleanValue = effect.CampaignEventDefinition.EventType == CampaignEventType.BooleanFlag ? effect.BooleanValue : null;
            state.SelectedOptionId = effect.CampaignEventDefinition.EventType == CampaignEventType.SingleChoice ? effect.SelectedOptionId : null;
            state.TextValue = effect.CampaignEventDefinition.EventType == CampaignEventType.TextValue ? effect.TextValue : null;
            state.NumericValue = effect.CampaignEventDefinition.EventType == CampaignEventType.NumericValue ? effect.NumericValue : null;
        }
        else if (effect.OperationType == OutcomeOperationType.Increment)
        {
            state.BooleanValue = null;
            state.SelectedOptionId = null;
            state.TextValue = null;
            state.NumericValue = (state.NumericValue ?? 0) + effect.NumericValue;
        }
        else if (effect.OperationType == OutcomeOperationType.Decrement)
        {
            state.BooleanValue = null;
            state.SelectedOptionId = null;
            state.TextValue = null;
            state.NumericValue = (state.NumericValue ?? 0) - effect.NumericValue;
        }

        state.SourceStoryBlockId = sourceType == OutcomeSourceType.StoryBlock ? sourceId : null;
        state.SourceStoryBeatId = sourceStoryBeatId;
    }

    private static int CompareState(
        CampaignEventState state,
        ConditionClause clause,
        CampaignEventType eventType)
    {
        return eventType switch
        {
            CampaignEventType.BooleanFlag => Nullable.Compare(state.BooleanValue, clause.BooleanValue),
            CampaignEventType.SingleChoice => Nullable.Compare(state.SelectedOptionId, clause.ExpectedOptionId),
            CampaignEventType.TextValue => string.Compare(
                state.TextValue,
                clause.TextValue,
                StringComparison.OrdinalIgnoreCase),
            CampaignEventType.NumericValue => Nullable.Compare(state.NumericValue, clause.NumericValue),
            _ => -1
        };
    }

    private static bool StateHasValue(CampaignEventState? state, CampaignEventType eventType)
    {
        return eventType switch
        {
            CampaignEventType.BooleanFlag => state?.BooleanValue is not null,
            CampaignEventType.SingleChoice => state?.SelectedOptionId is not null,
            CampaignEventType.TextValue => !string.IsNullOrWhiteSpace(state?.TextValue),
            CampaignEventType.NumericValue => state?.NumericValue is not null,
            _ => false
        };
    }

    private static string BuildFailureExplanation(
        CampaignEventDefinition definition,
        ConditionClause clause,
        CampaignEventState? state)
    {
        if (!StateHasValue(state, definition.EventType))
        {
            return $"{definition.Key} is not set";
        }

        return $"{definition.Key} must satisfy {clause.ComparisonOperator}, but its current value is {CurrentValueText(state!, definition.EventType)}";
    }

    private static string CurrentValueText(CampaignEventState state, CampaignEventType eventType)
    {
        return eventType switch
        {
            CampaignEventType.BooleanFlag => state.BooleanValue?.ToString() ?? "unset",
            CampaignEventType.SingleChoice => state.SelectedOption?.Key ?? state.SelectedOptionId?.ToString() ?? "unset",
            CampaignEventType.TextValue => state.TextValue ?? "unset",
            CampaignEventType.NumericValue => state.NumericValue?.ToString() ?? "unset",
            _ => "unset"
        };
    }

    private static IEnumerable<EvaluatedConditionClauseResponse> FlattenFailedClauses(
        EvaluatedConditionGroupResponse group)
    {
        foreach (var clause in group.Clauses.Where(clause => !clause.IsSatisfied))
        {
            yield return clause;
        }

        foreach (var clause in group.Groups.SelectMany(FlattenFailedClauses))
        {
            yield return clause;
        }
    }

    private static string? NormalizeKey(string? key)
    {
        var normalized = key?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Any(character => !char.IsLetterOrDigit(character)
                && character is not '_' and not '-'))
        {
            return null;
        }

        return normalized;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CampaignEventDefinitionResponse ToResponse(CampaignEventDefinition definition)
    {
        return new CampaignEventDefinitionResponse
        {
            Id = definition.Id,
            CampaignId = definition.CampaignId,
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            EventType = definition.EventType,
            IsRepeatable = definition.IsRepeatable,
            CreatedAtUtc = definition.CreatedAtUtc,
            UpdatedAtUtc = definition.UpdatedAtUtc,
            Options = definition.Options
                .OrderBy(option => option.SortOrder)
                .Select(ToResponse)
                .ToList()
        };
    }

    private static CampaignEventOptionResponse ToResponse(CampaignEventOption option)
    {
        return new CampaignEventOptionResponse
        {
            Id = option.Id,
            CampaignEventDefinitionId = option.CampaignEventDefinitionId,
            Key = option.Key,
            Label = option.Label,
            Description = option.Description,
            SortOrder = option.SortOrder
        };
    }

    private static CampaignEventStateResponse ToResponse(CampaignEventState state)
    {
        return new CampaignEventStateResponse
        {
            Id = state.Id,
            CampaignSessionId = state.CampaignSessionId,
            CampaignEventDefinitionId = state.CampaignEventDefinitionId,
            EventKey = state.CampaignEventDefinition.Key,
            EventType = state.CampaignEventDefinition.EventType,
            BooleanValue = state.BooleanValue,
            SelectedOptionId = state.SelectedOptionId,
            SelectedOptionKey = state.SelectedOption?.Key,
            TextValue = state.TextValue,
            NumericValue = state.NumericValue,
            SourceStoryBlockId = state.SourceStoryBlockId,
            SourceStoryBeatId = state.SourceStoryBeatId,
            ResolvedAtUtc = state.ResolvedAtUtc,
            UpdatedAtUtc = state.UpdatedAtUtc
        };
    }

    private static ConditionGroupResponse ToResponse(
        ConditionGroup group,
        IReadOnlyDictionary<Guid, List<ConditionGroup>> childrenByParentId)
    {
        return new ConditionGroupResponse
        {
            Id = group.Id,
            Operator = group.Operator,
            Negate = group.Negate,
            SortOrder = group.SortOrder,
            Clauses = group.Clauses
                .OrderBy(clause => clause.SortOrder)
                .Select(ToResponse)
                .ToList(),
            Groups = childrenByParentId.TryGetValue(group.Id, out var children)
                ? children
                    .OrderBy(child => child.SortOrder)
                    .Select(child => ToResponse(child, childrenByParentId))
                    .ToList()
                : []
        };
    }

    private static ConditionClauseResponse ToResponse(ConditionClause clause)
    {
        return new ConditionClauseResponse
        {
            Id = clause.Id,
            EventDefinitionId = clause.CampaignEventDefinitionId,
            EventKey = clause.CampaignEventDefinition.Key,
            ComparisonOperator = clause.ComparisonOperator,
            BooleanValue = clause.BooleanValue,
            ExpectedOptionId = clause.ExpectedOptionId,
            TextValue = clause.TextValue,
            NumericValue = clause.NumericValue,
            SortOrder = clause.SortOrder
        };
    }

    private static StoryOutcomeEffectResponse ToResponse(StoryOutcomeEffect effect)
    {
        return new StoryOutcomeEffectResponse
        {
            Id = effect.Id,
            CampaignId = effect.CampaignId,
            SourceType = effect.SourceType,
            SourceId = effect.SourceId,
            EventDefinitionId = effect.CampaignEventDefinitionId,
            EventKey = effect.CampaignEventDefinition.Key,
            OperationType = effect.OperationType,
            BooleanValue = effect.BooleanValue,
            SelectedOptionId = effect.SelectedOptionId,
            TextValue = effect.TextValue,
            NumericValue = effect.NumericValue,
            SortOrder = effect.SortOrder
        };
    }

    private static CampaignChoiceDefinitionResponse ToResponse(CampaignChoiceDefinition choice)
    {
        return new CampaignChoiceDefinitionResponse
        {
            Id = choice.Id,
            CampaignId = choice.CampaignId,
            StoryBlockId = choice.StoryBlockId,
            StoryBeatId = choice.StoryBeatId,
            Name = choice.Name,
            SelectionMode = choice.SelectionMode,
            Options = choice.Options
                .OrderBy(option => option.SortOrder)
                .Select(ToResponse)
                .ToList()
        };
    }

    private static CampaignChoiceOptionResponse ToResponse(CampaignChoiceOption option)
    {
        return new CampaignChoiceOptionResponse
        {
            Id = option.Id,
            CampaignChoiceDefinitionId = option.CampaignChoiceDefinitionId,
            StoryBeatId = option.StoryBeatId,
            Key = option.Key,
            Label = option.Label,
            Description = option.Description,
            SortOrder = option.SortOrder
        };
    }

    private static CampaignChoiceSelectionResponse ToResponse(CampaignChoiceSelection selection)
    {
        return new CampaignChoiceSelectionResponse
        {
            Id = selection.Id,
            CampaignSessionId = selection.CampaignSessionId,
            CampaignChoiceDefinitionId = selection.CampaignChoiceDefinitionId,
            CampaignChoiceOptionId = selection.CampaignChoiceOptionId,
            SelectedAtUtc = selection.SelectedAtUtc
        };
    }
}


