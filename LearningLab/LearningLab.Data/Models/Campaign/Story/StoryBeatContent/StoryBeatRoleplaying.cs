using System.ComponentModel.DataAnnotations;
using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatRoleplaying
{
    public string MainDescription { get; set; } = string.Empty;

    public List<StoryBeatRoleplayingNpcReference> NpcReferences { get; set; } = [];

    public List<StoryBeatRoleplayingInformation> DiscoverableInformation { get; set; } = [];
}

public class StoryBeatRoleplayingNpcReference
{
    public string NpcTag { get; set; } = string.Empty;
}

public class StoryBeatRoleplayingInformation
{
    public string NpcTag { get; set; } = string.Empty;

    public StoryBeatRoleplayingCheckType CheckType { get; set; }

    public Skill? Skill { get; set; }

    public Ability? Ability { get; set; }

    [Range(1, 30)]
    public int? DifficultyClass { get; set; }

    public string Information { get; set; } = string.Empty;
}

public enum StoryBeatRoleplayingCheckType
{
    None,
    Skill,
    Ability
}
