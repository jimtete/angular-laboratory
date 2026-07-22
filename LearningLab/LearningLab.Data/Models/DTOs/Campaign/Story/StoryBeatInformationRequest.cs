namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatInformationRequest
{
    public string? Narrative { get; init; }

    public IReadOnlyList<StoryBeatOptionalInformationRequest> OptionalInformation { get; init; } = [];
}
