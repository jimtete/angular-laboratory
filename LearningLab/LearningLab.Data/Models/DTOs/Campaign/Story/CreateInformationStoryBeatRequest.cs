namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateInformationStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public StoryBeatInformationRequest? Information { get; init; }
}
