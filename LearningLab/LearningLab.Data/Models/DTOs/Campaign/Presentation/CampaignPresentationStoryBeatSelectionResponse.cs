namespace LearningLab.Data.Models.DTOs.Campaign.Presentation;

public sealed class CampaignPresentationStoryBeatSelectionResponse
{
    public long Id { get; init; }
    public int CampaignPresentationId { get; init; }
    public Guid StoryBlockId { get; init; }
    public int OrderIndex { get; init; }
    public int SelectedSecondaryOrderIndex { get; init; }
    public Guid SelectedStoryBeatId { get; init; }
    public DateTimeOffset SelectedAt { get; init; }
}
