namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateInformationStoryBeatRequest
{
    public string? Title { get; init; }

    public StoryBeatInformationRequest? Information { get; init; }
}
