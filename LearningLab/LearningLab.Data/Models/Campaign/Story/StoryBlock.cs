namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBlock
{
    public Guid StoryBlockId { get; set; }
    
    public Campaign Campaign { get; set; } = null!;
    public Guid CampaignId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ICollection<StoryBeat> Beats { get; set; } = [];
    public ICollection<StoryBlockMilestone> Milestones { get; set; } = [];
}
