using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Presentation;

public class CampaignPresentation
{
    public int Id { get; set; }

    public int CampaignSessionId { get; set; }
    public CampaignSession CampaignSession { get; set; } = null!;

    public PresentationStatus Status { get; set; }

    public Guid? ActiveStoryBlockId { get; set; }
    public StoryBlock? ActiveStoryBlock { get; set; }

    public Guid? CurrentStoryBeatId { get; set; }
    public StoryBeat? CurrentStoryBeat { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public ICollection<CampaignPresentationEntry> Entries { get; set; } = [];
    public ICollection<CampaignPresentationStoryBeatSelection> StoryBeatSelections { get; set; } = [];
}
