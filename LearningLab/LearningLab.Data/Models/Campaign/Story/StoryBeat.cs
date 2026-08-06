using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Campaign.Presentation;
using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeat
{
    public Guid Id { get; set; }

    public string Title { get; set; } =  string.Empty;
    
    public StoryBlock StoryBlock { get; set; } = null!;
    public Guid StoryBlockId { get; set; }

    public int OrderIndex { get; set; }

    public int SecondaryOrderIndex { get; set; } = 1;

    public StoryBeatType StoryBeatType { get; set; }
    public StoryBeatInformation? Information { get; set; }
    public StoryBeatNarrative? Narrative { get; set; }
    public StoryBeatRoleplaying? Roleplaying { get; set; }
    public StoryBeatDecision? Decision { get; set; }
    public StoryBeatCombat? Combat { get; set; }
    public StoryBeatTransition? Transition { get; set; }

    public int? CampaignMilestoneId { get; set; }
    public CampaignMilestone? Milestone { get; set; }
    public ICollection<CampaignPresentation> CurrentPresentations { get; set; } = [];
    public ICollection<CampaignPresentationEntry> PresentationEntries { get; set; } = [];
    public ICollection<CampaignPresentationStoryBeatSelection> SelectedInPresentations { get; set; } = [];
    public ICollection<StoryBeatQuestTask> QuestTaskLinks { get; set; } = [];
    public ICollection<StoryBlockMusicFile> MusicFiles { get; set; } = [];
}
