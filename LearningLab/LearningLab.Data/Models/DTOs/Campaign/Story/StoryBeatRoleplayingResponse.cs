namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatRoleplayingResponse
{
    public required string MainDescription { get; init; }

    public IReadOnlyList<string> NpcTags { get; init; } = [];

    public IReadOnlyList<StoryBeatRoleplayingInformationResponse> DiscoverableInformation { get; init; } = [];
}
