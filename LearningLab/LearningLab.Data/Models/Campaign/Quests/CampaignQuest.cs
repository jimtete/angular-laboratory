namespace LearningLab.Data.Models.Campaign.Quests;

public class CampaignQuest
{
    public Guid QuestId { get; set; }
    public Guid CampaignId { get; set; }
    public CampaignQuestType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GivenBy { get; set; } = string.Empty;
    public string Reward { get; set; } = string.Empty;
    public ICollection<CampaignQuestTask> Tasks { get; set; } = [];
    public DateTimeOffset? CompletedAt { get; set; } = null;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Campaign Campaign { get; set; } = null!;
}
