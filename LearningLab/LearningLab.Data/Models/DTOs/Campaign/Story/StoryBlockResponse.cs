using LearningLab.Data.Models.DTOs.Campaign.Maps;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBlockResponse
{
    public Guid StoryBlockId { get; init; }

    public Guid CampaignId { get; init; }

    public required string Title { get; init; }

    public int OrderIndex { get; init; }

    public IReadOnlyList<MapPinResponse> MapPins { get; init; } = [];

    public IReadOnlyList<StoryBlockMusicFileResponse> MusicFiles { get; init; } = [];
}
