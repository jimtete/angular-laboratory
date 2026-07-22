using System.ComponentModel.DataAnnotations;
using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatInformation
{
    public string Narrative { get; set; } = string.Empty;

    public List<StoryBeatOptionalInformation> OptionalInformation { get; set; } = [];
}

public class StoryBeatOptionalInformation
{
    public Skill Skill { get; set; }

    [Range(1, 30)]
    public int DifficultyClass { get; set; }

    public string Information { get; set; } = string.Empty;

    public StoryBeatOptionalInformationPlacement Placement { get; set; }

    public int? NarrativeOffset { get; set; }
}

public enum StoryBeatOptionalInformationPlacement
{
    Appended,
    Inline
}
