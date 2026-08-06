using LearningLab.Data.Models.Campaign.Presentation;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBlock
{
    public Guid StoryBlockId { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Guid CampaignId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public ICollection<StoryBeat> Beats { get; set; } = [];
    public ICollection<StoryBlockMusicFile> MusicFiles { get; set; } = [];
    public ICollection<StoryBlockMilestone> Milestones { get; set; } = [];
    public ICollection<StoryBeatIndexPathRule> IndexPathRules { get; set; } = [];
    public ICollection<CampaignPresentation> ActivePresentations { get; set; } = [];
    public ICollection<CampaignPresentationEntry> PresentationEntries { get; set; } = [];
    public ICollection<CampaignPresentationStoryBeatSelection> PresentationStoryBeatSelections { get; set; } = [];
}
