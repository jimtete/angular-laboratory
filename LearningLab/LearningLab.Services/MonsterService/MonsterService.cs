using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Monsters;
using LearningLab.Data.Models.Monsters;
using LearningLab.Data.Repositories.MonsterRepository;

namespace LearningLab.Services.MonsterService;

public sealed class MonsterService : IMonsterService
{
    private const int MaximumNameLength = 256;
    private const int MaximumShortTextLength = 128;

    private readonly IMonsterRepository _monsterRepository;

    public MonsterService(IMonsterRepository monsterRepository)
    {
        _monsterRepository = monsterRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<MonsterListResponse>>> GetMonstersAsync(
        CancellationToken cancellationToken = default)
    {
        var monsters = await _monsterRepository.ListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<MonsterListResponse>>(
            ApplicationStatusCode.Success,
            monsters.Select(ToListResponse).ToList());
    }

    public async Task<ServiceResult<MonsterResponse>> GetMonsterByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1)
        {
            return new ServiceResult<MonsterResponse>(ApplicationStatusCode.InvalidMonster);
        }

        var monster = await _monsterRepository.GetByIdAsync(monsterId, cancellationToken);

        return monster is null
            ? new ServiceResult<MonsterResponse>(ApplicationStatusCode.MonsterNotFound)
            : new ServiceResult<MonsterResponse>(
                ApplicationStatusCode.Success,
                ToResponse(monster));
    }

    public async Task<ServiceResult<MonsterResponse>> CreateMonsterAsync(
        CreateMonsterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildMonster(request, out var monster))
        {
            return new ServiceResult<MonsterResponse>(ApplicationStatusCode.InvalidMonster);
        }

        await _monsterRepository.AddAsync(monster, cancellationToken);
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterResponse>(
            ApplicationStatusCode.Success,
            ToResponse(monster));
    }

    public async Task<ServiceResult<MonsterResponse>> UpdateMonsterBasicInformationAsync(
        int monsterId,
        UpdateMonsterBasicInformationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || !TryBuildMonsterBasics(request, out var basics))
        {
            return new ServiceResult<MonsterResponse>(ApplicationStatusCode.InvalidMonster);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<MonsterResponse>(ApplicationStatusCode.MonsterNotFound);
        }

        monster.Name = basics.Name;
        monster.Size = basics.Size;
        monster.Race = basics.Race;
        monster.Class = basics.Class;
        monster.Tags = basics.Tags;
        monster.Notes = basics.Notes;
        monster.Abilities.Clear();
        monster.Proficiencies.Clear();

        foreach (var ability in basics.Abilities)
        {
            monster.Abilities.Add(ability);
        }

        foreach (var proficiency in basics.Proficiencies)
        {
            monster.Proficiencies.Add(proficiency);
        }

        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterResponse>(
            ApplicationStatusCode.Success,
            ToResponse(monster));
    }

    public async Task<ServiceResult<bool>> DeleteMonsterAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.InvalidMonster);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.MonsterNotFound);
        }

        _monsterRepository.Remove(monster);
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(ApplicationStatusCode.Success, true);
    }

    public async Task<ServiceResult<MonsterFeatureResponse>> AddMonsterFeatureAsync(
        int monsterId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || !TryBuildFeature(request, out var feature))
        {
            return new ServiceResult<MonsterFeatureResponse>(ApplicationStatusCode.InvalidMonsterFeature);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<MonsterFeatureResponse>(ApplicationStatusCode.MonsterNotFound);
        }

        if (feature.SortOrder < 1)
        {
            feature.SortOrder = GetNextFeatureSortOrder(monster.Features);
        }

        monster.Features.Add(feature);
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterFeatureResponse>(
            ApplicationStatusCode.Success,
            ToResponse(feature));
    }

    public async Task<ServiceResult<MonsterFeatureResponse>> UpdateMonsterFeatureAsync(
        int monsterId,
        int featureId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || featureId < 1 || !TryBuildFeature(request, out var updatedFeature))
        {
            return new ServiceResult<MonsterFeatureResponse>(ApplicationStatusCode.InvalidMonsterFeature);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<MonsterFeatureResponse>(ApplicationStatusCode.MonsterNotFound);
        }

        var feature = monster.Features.SingleOrDefault(feature => feature.Id == featureId);

        if (feature is null)
        {
            return new ServiceResult<MonsterFeatureResponse>(ApplicationStatusCode.MonsterFeatureNotFound);
        }

        ApplyFeature(feature, updatedFeature);
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterFeatureResponse>(
            ApplicationStatusCode.Success,
            ToResponse(feature));
    }

    public async Task<ServiceResult<bool>> DeleteMonsterFeatureAsync(
        int monsterId,
        int featureId,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || featureId < 1)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.InvalidMonsterFeature);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.MonsterNotFound);
        }

        var feature = monster.Features.SingleOrDefault(feature => feature.Id == featureId);

        if (feature is null)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.MonsterFeatureNotFound);
        }

        monster.Features.Remove(feature);
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(ApplicationStatusCode.Success, true);
    }

    public async Task<ServiceResult<IReadOnlyList<MonsterFeatureResponse>>> ReorderMonsterFeaturesAsync(
        int monsterId,
        ReorderMonsterFeaturesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1
            || request?.Features is null
            || request.Features.Count == 0
            || request.Features.Any(feature => feature.FeatureId < 1)
            || request.Features.Select(feature => feature.FeatureId).Distinct().Count() != request.Features.Count)
        {
            return new ServiceResult<IReadOnlyList<MonsterFeatureResponse>>(
                ApplicationStatusCode.InvalidMonsterFeature);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<IReadOnlyList<MonsterFeatureResponse>>(
                ApplicationStatusCode.MonsterNotFound);
        }

        var featuresById = monster.Features.ToDictionary(feature => feature.Id);

        foreach (var featureOrder in request.Features)
        {
            if (!featuresById.TryGetValue(featureOrder.FeatureId, out var feature))
            {
                return new ServiceResult<IReadOnlyList<MonsterFeatureResponse>>(
                    ApplicationStatusCode.MonsterFeatureNotFound);
            }

            feature.SortOrder = featureOrder.SortOrder;
        }

        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<MonsterFeatureResponse>>(
            ApplicationStatusCode.Success,
            monster.Features
                .OrderBy(feature => feature.SortOrder)
                .ThenBy(feature => feature.Id)
                .Select(ToResponse)
                .ToList());
    }

    public async Task<ServiceResult<MonsterSpellcastingResponse>> UpsertMonsterSpellcastingAsync(
        int monsterId,
        MonsterSpellcastingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || !TryBuildSpellcasting(request, out var spellcasting))
        {
            return new ServiceResult<MonsterSpellcastingResponse>(
                ApplicationStatusCode.InvalidMonsterSpellcasting);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<MonsterSpellcastingResponse>(ApplicationStatusCode.MonsterNotFound);
        }

        if (monster.Spellcasting is null)
        {
            monster.Spellcasting = spellcasting;
        }
        else
        {
            monster.Spellcasting.SpellcastingAbility = spellcasting.SpellcastingAbility;
            monster.Spellcasting.SpellSaveDC = spellcasting.SpellSaveDC;
            monster.Spellcasting.SpellAttackBonus = spellcasting.SpellAttackBonus;
            monster.Spellcasting.Notes = spellcasting.Notes;
            monster.Spellcasting.SpellSlots.Clear();

            foreach (var spellSlot in spellcasting.SpellSlots)
            {
                monster.Spellcasting.SpellSlots.Add(spellSlot);
            }
        }

        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterSpellcastingResponse>(
            ApplicationStatusCode.Success,
            ToResponse(monster.Spellcasting));
    }

    public async Task<ServiceResult<bool>> RemoveMonsterSpellcastingAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.InvalidMonsterSpellcasting);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.MonsterNotFound);
        }

        if (monster.Spellcasting is null)
        {
            return new ServiceResult<bool>(ApplicationStatusCode.MonsterSpellcastingNotFound);
        }

        monster.Spellcasting = null;
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<bool>(ApplicationStatusCode.Success, true);
    }

    public async Task<ServiceResult<MonsterSpellcastingResponse>> UpdateRemainingSpellSlotsAsync(
        int monsterId,
        int spellLevel,
        UpdateRemainingSpellSlotsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (monsterId < 1 || spellLevel < 0 || request is null || request.RemainingSlots is < 0)
        {
            return new ServiceResult<MonsterSpellcastingResponse>(
                ApplicationStatusCode.InvalidMonsterSpellSlot);
        }

        var monster = await _monsterRepository.GetMutableByIdAsync(monsterId, cancellationToken);

        if (monster is null)
        {
            return new ServiceResult<MonsterSpellcastingResponse>(ApplicationStatusCode.MonsterNotFound);
        }

        if (monster.Spellcasting is null)
        {
            return new ServiceResult<MonsterSpellcastingResponse>(
                ApplicationStatusCode.MonsterSpellcastingNotFound);
        }

        var slot = monster.Spellcasting.SpellSlots.SingleOrDefault(slot => slot.SpellLevel == spellLevel);

        if (slot is null)
        {
            return new ServiceResult<MonsterSpellcastingResponse>(
                ApplicationStatusCode.MonsterSpellSlotNotFound);
        }

        slot.RemainingSlots = request!.RemainingSlots;
        await _monsterRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<MonsterSpellcastingResponse>(
            ApplicationStatusCode.Success,
            ToResponse(monster.Spellcasting));
    }

    private static bool TryBuildMonster(
        CreateMonsterRequest? request,
        out Monster monster)
    {
        monster = new Monster();

        if (!TryBuildMonsterBasics(request, out var basics))
        {
            return false;
        }

        if (!TryBuildFeatures(request?.Features, out var features)
            || !TryBuildSpellcasting(request?.Spellcasting, out var spellcasting))
        {
            return false;
        }

        monster = new Monster
        {
            Name = basics.Name,
            Size = basics.Size,
            Race = basics.Race,
            Class = basics.Class,
            Tags = basics.Tags,
            Abilities = basics.Abilities,
            Proficiencies = basics.Proficiencies,
            Spellcasting = request?.Spellcasting is null ? null : spellcasting,
            Features = features,
            Notes = basics.Notes
        };

        return true;
    }

    private static bool TryBuildMonsterBasics(
        CreateMonsterRequest? request,
        out Monster monster)
    {
        monster = new Monster();

        var name = request?.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumNameLength)
        {
            return false;
        }

        if (!TryNormalizeShortText(request?.Size, out var size)
            || !TryNormalizeShortText(request?.Race, out var race)
            || !TryNormalizeShortText(request?.Class, out var monsterClass)
            || !TryBuildAbilities(request?.Abilities, out var abilities)
            || !TryBuildProficiencies(request?.Proficiencies, out var proficiencies))
        {
            return false;
        }

        monster = new Monster
        {
            Name = name,
            Size = size,
            Race = race,
            Class = monsterClass,
            Tags = NormalizeTags(request?.Tags),
            Abilities = abilities,
            Proficiencies = proficiencies,
            Notes = NormalizeText(request?.Notes)
        };

        return true;
    }

    private static bool TryBuildMonsterBasics(
        UpdateMonsterBasicInformationRequest? request,
        out Monster monster)
    {
        monster = new Monster();

        var name = request?.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumNameLength)
        {
            return false;
        }

        if (!TryNormalizeShortText(request?.Size, out var size)
            || !TryNormalizeShortText(request?.Race, out var race)
            || !TryNormalizeShortText(request?.Class, out var monsterClass)
            || !TryBuildAbilities(request?.Abilities, out var abilities)
            || !TryBuildProficiencies(request?.Proficiencies, out var proficiencies))
        {
            return false;
        }

        monster = new Monster
        {
            Name = name,
            Size = size,
            Race = race,
            Class = monsterClass,
            Tags = NormalizeTags(request?.Tags),
            Abilities = abilities,
            Proficiencies = proficiencies,
            Notes = NormalizeText(request?.Notes)
        };

        return true;
    }

    private static bool TryBuildAbilities(
        IReadOnlyCollection<MonsterAbilityRequest>? requests,
        out List<MonsterAbility> abilities)
    {
        abilities = [];

        if (requests is null)
        {
            return true;
        }

        foreach (var request in requests)
        {
            if (request is null)
            {
                return false;
            }

            var name = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumShortTextLength)
            {
                return false;
            }

            abilities.Add(new MonsterAbility
            {
                Name = name,
                Value = request.Value,
                Modifier = request.Modifier,
                Notes = NormalizeText(request.Notes)
            });
        }

        return true;
    }

    private static bool TryBuildProficiencies(
        IReadOnlyCollection<MonsterProficiencyRequest>? requests,
        out List<MonsterProficiency> proficiencies)
    {
        proficiencies = [];

        if (requests is null)
        {
            return true;
        }

        foreach (var request in requests)
        {
            if (request is null)
            {
                return false;
            }

            var name = request.Name?.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumShortTextLength)
            {
                return false;
            }

            proficiencies.Add(new MonsterProficiency
            {
                Name = name,
                Bonus = request.Bonus,
                Notes = NormalizeText(request.Notes)
            });
        }

        return true;
    }

    private static bool TryBuildFeatures(
        IReadOnlyList<MonsterFeatureRequest>? requests,
        out List<MonsterFeature> features)
    {
        features = [];

        if (requests is null)
        {
            return true;
        }

        for (var index = 0; index < requests.Count; index++)
        {
            if (!TryBuildFeature(requests[index], out var feature))
            {
                return false;
            }

            if (feature.SortOrder < 1)
            {
                feature.SortOrder = index + 1;
            }

            features.Add(feature);
        }

        return true;
    }

    private static bool TryBuildFeature(
        MonsterFeatureRequest? request,
        out MonsterFeature feature)
    {
        feature = new MonsterFeature();

        var name = request?.Name?.Trim();

        if (request is null
            || string.IsNullOrWhiteSpace(name)
            || name.Length > MaximumNameLength
            || !Enum.IsDefined(request.Category)
            || !TryNormalizeShortText(request.CastingTime, out var castingTime)
            || !TryNormalizeShortText(request.Range, out var range)
            || !TryNormalizeShortText(request.Duration, out var duration)
            || request.ResourceCost is < 0
            || request.SpellLevel is < 0)
        {
            return false;
        }

        feature = new MonsterFeature
        {
            Name = name,
            Description = NormalizeText(request.Description),
            Category = request.Category,
            UsageNote = NormalizeText(request.UsageNote),
            ResourceCost = request.ResourceCost,
            IsSpell = request.IsSpell,
            SpellLevel = request.IsSpell ? request.SpellLevel : null,
            CastingTime = request.IsSpell ? castingTime : null,
            Range = request.IsSpell ? range : null,
            Duration = request.IsSpell ? duration : null,
            Concentration = request.IsSpell ? request.Concentration : null,
            SortOrder = request.SortOrder
        };

        return true;
    }

    private static bool TryBuildSpellcasting(
        MonsterSpellcastingRequest? request,
        out MonsterSpellcasting spellcasting)
    {
        spellcasting = new MonsterSpellcasting();

        if (request is null)
        {
            return true;
        }

        if (!TryNormalizeShortText(request.SpellcastingAbility, out var spellcastingAbility)
            || request.SpellSaveDC is < 0
            || !TryBuildSpellSlots(request.SpellSlots, out var spellSlots))
        {
            return false;
        }

        spellcasting = new MonsterSpellcasting
        {
            SpellcastingAbility = spellcastingAbility,
            SpellSaveDC = request.SpellSaveDC,
            SpellAttackBonus = request.SpellAttackBonus,
            Notes = NormalizeText(request.Notes),
            SpellSlots = spellSlots
        };

        return true;
    }

    private static bool TryBuildSpellSlots(
        IReadOnlyCollection<MonsterSpellSlotRequest>? requests,
        out List<MonsterSpellSlot> spellSlots)
    {
        spellSlots = [];

        if (requests is null)
        {
            return true;
        }

        var spellLevels = new HashSet<int>();

        foreach (var request in requests)
        {
            if (request is null
                || request.SpellLevel < 0
                || request.MaximumSlots is < 0
                || request.RemainingSlots is < 0
                || !spellLevels.Add(request.SpellLevel))
            {
                return false;
            }

            spellSlots.Add(new MonsterSpellSlot
            {
                SpellLevel = request.SpellLevel,
                MaximumSlots = request.MaximumSlots,
                RemainingSlots = request.RemainingSlots
            });
        }

        return true;
    }

    private static void ApplyFeature(
        MonsterFeature feature,
        MonsterFeature updatedFeature)
    {
        feature.Name = updatedFeature.Name;
        feature.Description = updatedFeature.Description;
        feature.Category = updatedFeature.Category;
        feature.UsageNote = updatedFeature.UsageNote;
        feature.ResourceCost = updatedFeature.ResourceCost;
        feature.IsSpell = updatedFeature.IsSpell;
        feature.SpellLevel = updatedFeature.SpellLevel;
        feature.CastingTime = updatedFeature.CastingTime;
        feature.Range = updatedFeature.Range;
        feature.Duration = updatedFeature.Duration;
        feature.Concentration = updatedFeature.Concentration;
        feature.SortOrder = updatedFeature.SortOrder;
    }

    private static int GetNextFeatureSortOrder(IEnumerable<MonsterFeature> features)
    {
        return features
            .Select(feature => (int?)feature.SortOrder)
            .Max() + 1
            ?? 1;
    }

    private static List<string>? NormalizeTags(List<string>? tags)
    {
        return tags?
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool TryNormalizeShortText(
        string? value,
        out string? normalizedValue)
    {
        normalizedValue = NormalizeText(value);

        return normalizedValue is null || normalizedValue.Length <= MaximumShortTextLength;
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static MonsterListResponse ToListResponse(Monster monster)
    {
        return new MonsterListResponse
        {
            Id = monster.Id,
            Name = monster.Name,
            Size = monster.Size,
            Race = monster.Race,
            Class = monster.Class,
            Tags = monster.Tags
        };
    }

    private static MonsterResponse ToResponse(Monster monster)
    {
        return new MonsterResponse
        {
            Id = monster.Id,
            Name = monster.Name,
            Size = monster.Size,
            Race = monster.Race,
            Class = monster.Class,
            Tags = monster.Tags,
            Abilities = monster.Abilities
                .OrderBy(ability => ability.Id)
                .Select(ToResponse)
                .ToList(),
            Proficiencies = monster.Proficiencies
                .OrderBy(proficiency => proficiency.Id)
                .Select(ToResponse)
                .ToList(),
            Spellcasting = monster.Spellcasting is null
                ? null
                : ToResponse(monster.Spellcasting),
            Features = monster.Features
                .OrderBy(feature => feature.SortOrder)
                .ThenBy(feature => feature.Id)
                .Select(ToResponse)
                .ToList(),
            Notes = monster.Notes
        };
    }

    private static MonsterAbilityResponse ToResponse(MonsterAbility ability)
    {
        return new MonsterAbilityResponse
        {
            Name = ability.Name,
            Value = ability.Value,
            Modifier = ability.Modifier,
            Notes = ability.Notes
        };
    }

    private static MonsterProficiencyResponse ToResponse(MonsterProficiency proficiency)
    {
        return new MonsterProficiencyResponse
        {
            Name = proficiency.Name,
            Bonus = proficiency.Bonus,
            Notes = proficiency.Notes
        };
    }

    private static MonsterFeatureResponse ToResponse(MonsterFeature feature)
    {
        return new MonsterFeatureResponse
        {
            Id = feature.Id,
            Name = feature.Name,
            Description = feature.Description,
            Category = feature.Category,
            UsageNote = feature.UsageNote,
            ResourceCost = feature.ResourceCost,
            IsSpell = feature.IsSpell,
            SpellLevel = feature.SpellLevel,
            CastingTime = feature.CastingTime,
            Range = feature.Range,
            Duration = feature.Duration,
            Concentration = feature.Concentration,
            SortOrder = feature.SortOrder
        };
    }

    private static MonsterSpellcastingResponse ToResponse(MonsterSpellcasting spellcasting)
    {
        return new MonsterSpellcastingResponse
        {
            SpellcastingAbility = spellcasting.SpellcastingAbility,
            SpellSaveDC = spellcasting.SpellSaveDC,
            SpellAttackBonus = spellcasting.SpellAttackBonus,
            Notes = spellcasting.Notes,
            SpellSlots = spellcasting.SpellSlots
                .OrderBy(slot => slot.SpellLevel)
                .Select(ToResponse)
                .ToList()
        };
    }

    private static MonsterSpellSlotResponse ToResponse(MonsterSpellSlot slot)
    {
        return new MonsterSpellSlotResponse
        {
            SpellLevel = slot.SpellLevel,
            MaximumSlots = slot.MaximumSlots,
            RemainingSlots = slot.RemainingSlots
        };
    }
}
