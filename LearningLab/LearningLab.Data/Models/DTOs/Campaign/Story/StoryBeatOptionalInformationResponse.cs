using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatOptionalInformationResponse
{
    public Skill Skill { get; init; }

    public int DifficultyClass { get; init; }

    public required string Information { get; init; }

    public StoryBeatOptionalInformationPlacement Placement { get; init; }

    public int? NarrativeOffset { get; init; }
}
