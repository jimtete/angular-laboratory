namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatCombatResponse
{
    public required string Description { get; init; }

    public string? Rewards { get; init; }

    public IReadOnlyList<StoryBeatCombatEnemyNpcResponse> EnemyNpcs { get; init; } = [];
}
