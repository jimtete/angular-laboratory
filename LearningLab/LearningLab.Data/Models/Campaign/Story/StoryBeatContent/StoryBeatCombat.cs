namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatCombat
{
    public string Description { get; set; } = string.Empty;

    public string? Rewards { get; set; }

    public List<StoryBeatCombatEnemyNpc> EnemyNpcs { get; set; } = [];
}

public class StoryBeatCombatEnemyNpc
{
    public int MonsterId { get; set; }

    public int Amount { get; set; }
}
