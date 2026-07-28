using System.Text.Json.Serialization;
using LearningLab.Data.Serialization;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatCombatRequest
{
    public string? Description { get; init; }

    [JsonConverter(typeof(StringOrStringArrayJsonConverter))]
    public string? Rewards { get; init; }

    public IReadOnlyList<StoryBeatCombatEnemyNpcRequest> EnemyNpcs { get; init; } = [];
}
