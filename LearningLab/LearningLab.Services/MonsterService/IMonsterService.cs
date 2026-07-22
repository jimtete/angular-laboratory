using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Monsters;

namespace LearningLab.Services.MonsterService;

public interface IMonsterService
{
    Task<ServiceResult<IReadOnlyList<MonsterListResponse>>> GetMonstersAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterResponse>> GetMonsterByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterResponse>> CreateMonsterAsync(
        CreateMonsterRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterResponse>> UpdateMonsterBasicInformationAsync(
        int monsterId,
        UpdateMonsterBasicInformationRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteMonsterAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterFeatureResponse>> AddMonsterFeatureAsync(
        int monsterId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterFeatureResponse>> UpdateMonsterFeatureAsync(
        int monsterId,
        int featureId,
        MonsterFeatureRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> DeleteMonsterFeatureAsync(
        int monsterId,
        int featureId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<MonsterFeatureResponse>>> ReorderMonsterFeaturesAsync(
        int monsterId,
        ReorderMonsterFeaturesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterSpellcastingResponse>> UpsertMonsterSpellcastingAsync(
        int monsterId,
        MonsterSpellcastingRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RemoveMonsterSpellcastingAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MonsterSpellcastingResponse>> UpdateRemainingSpellSlotsAsync(
        int monsterId,
        int spellLevel,
        UpdateRemainingSpellSlotsRequest request,
        CancellationToken cancellationToken = default);
}
