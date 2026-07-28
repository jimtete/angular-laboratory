using LearningLab.Data.Models.Campaign.Presentation;

namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class CampaignPresentationResponse
{
    public int Id { get; init; }
    public int CampaignSessionId { get; init; }
    public PresentationStatus Status { get; init; }
    public Guid? ActiveStoryBlockId { get; init; }
    public Guid? CurrentStoryBeatId { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public CampaignPresentationEntryResponse? LatestEntry { get; init; }
    public IReadOnlyList<CampaignPresentationStoryBeatSelectionResponse> StoryBeatSelections { get; init; } = [];
}
