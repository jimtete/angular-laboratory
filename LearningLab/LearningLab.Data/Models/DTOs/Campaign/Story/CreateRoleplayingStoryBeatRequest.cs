namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateRoleplayingStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatRoleplayingRequest? Roleplaying { get; init; }
}
