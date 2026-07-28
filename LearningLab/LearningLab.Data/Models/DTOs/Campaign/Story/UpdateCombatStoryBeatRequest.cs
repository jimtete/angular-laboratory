namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateCombatStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatCombatRequest? Combat { get; init; }
}
