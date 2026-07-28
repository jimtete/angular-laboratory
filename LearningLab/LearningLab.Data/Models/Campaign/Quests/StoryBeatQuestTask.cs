using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Quests;

public class StoryBeatQuestTask
{
    public Guid StoryBeatId { get; set; }
    public StoryBeat StoryBeat { get; set; } = null!;

    public Guid QuestTaskId { get; set; }
    public CampaignQuestTask QuestTask { get; set; } = null!;

    public DateTimeOffset LinkedAt { get; set; }
}
