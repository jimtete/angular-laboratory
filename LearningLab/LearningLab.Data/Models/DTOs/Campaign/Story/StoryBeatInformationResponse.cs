namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatInformationResponse
{
    public required string Narrative { get; init; }

    public IReadOnlyList<StoryBeatOptionalInformationResponse> OptionalInformation { get; init; } = [];
}
