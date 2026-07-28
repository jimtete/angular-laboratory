using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Presentation;

public class CampaignPresentationEntry
{
    public long Id { get; set; }

    public int CampaignPresentationId { get; set; }
    public CampaignPresentation CampaignPresentation { get; set; } = null!;

    public int Sequence { get; set; }

    public PresentationEntryType EntryType { get; set; }

    public Guid StoryBlockId { get; set; }
    public StoryBlock StoryBlock { get; set; } = null!;

    public Guid? StoryBeatId { get; set; }
    public StoryBeat? StoryBeat { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
