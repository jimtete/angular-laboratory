namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatRoleplayingRequest
{
    public string? MainDescription { get; init; }

    public IReadOnlyList<string> NpcTags { get; init; } = [];

    public IReadOnlyList<StoryBeatRoleplayingInformationRequest> DiscoverableInformation { get; init; } = [];
}
