using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Presentation;

public class CampaignPresentationStoryBeatSelection
{
    public long Id { get; set; }

    public int CampaignPresentationId { get; set; }
    public CampaignPresentation CampaignPresentation { get; set; } = null!;

    public Guid StoryBlockId { get; set; }
    public StoryBlock StoryBlock { get; set; } = null!;

    public int OrderIndex { get; set; }

    public int SelectedSecondaryOrderIndex { get; set; }

    public Guid SelectedStoryBeatId { get; set; }
    public StoryBeat SelectedStoryBeat { get; set; } = null!;

    public DateTimeOffset SelectedAt { get; set; }
}
