namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateRoleplayingStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatRoleplayingRequest? Roleplaying { get; init; }
}
