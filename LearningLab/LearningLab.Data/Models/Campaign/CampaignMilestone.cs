using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign;

public class CampaignMilestone
{
    public int Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? AchievedAt { get; set; }
    public CampaignMilestoneImportance Importance { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public ICollection<StoryBlockMilestone> StoryBlockMilestones { get; set; } = [];
    public StoryBeat? StoryBeat { get; set; }
}
