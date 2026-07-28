using LearningLab.Data.Models.Campaign.Presentation;

namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class CampaignPresentationEntryResponse
{
    public long Id { get; init; }
    public int CampaignPresentationId { get; init; }
    public int Sequence { get; init; }
    public PresentationEntryType EntryType { get; init; }
    public Guid StoryBlockId { get; init; }
    public Guid? StoryBeatId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
