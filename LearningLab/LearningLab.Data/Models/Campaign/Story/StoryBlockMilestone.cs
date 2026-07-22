using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBlockMilestone
{
    public Guid StoryBlockId { get; set; }

    public StoryBlock StoryBlock { get; set; } = null!;

    public int CampaignMilestoneId { get; set; }

    public CampaignMilestone CampaignMilestone { get; set; } = null!;

    public int OrderIndex { get; set; }
}
