using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatRoleplayingInformationRequest
{
    public string? NpcTag { get; init; }

    public StoryBeatRoleplayingCheckType CheckType { get; init; }

    public Skill? Skill { get; init; }

    public Ability? Ability { get; init; }

    public int? DifficultyClass { get; init; }

    public string? Information { get; init; }
}
