using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeat
{
    public Guid Id { get; set; }

    public string Title { get; set; } =  string.Empty;
    
    public StoryBlock StoryBlock { get; set; } = null!;
    public Guid StoryBlockId { get; set; }

    public int OrderIndex { get; set; }

    public StoryBeatType StoryBeatType { get; set; }
    public StoryBeatInformation? Information { get; set; }
    public StoryBeatNarrative? Narrative { get; set; }
    public StoryBeatRoleplaying? Roleplaying { get; set; }
    public StoryBeatDecision? Decision { get; set; }

    public int? CampaignMilestoneId { get; set; }
    public CampaignMilestone? Milestone { get; set; }
}
