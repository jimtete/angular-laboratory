namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateCombatStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatCombatRequest? Combat { get; init; }
}
