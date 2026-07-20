namespace LearningLab.Data.Models.Campaign.Quests;

public class CampaignQuestTask
{
    public Guid QuestTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? DateCompleted { get; set; } = null;
    public Guid QuestId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public CampaignQuest CampaignQuest { get; set; } = null!;
}
