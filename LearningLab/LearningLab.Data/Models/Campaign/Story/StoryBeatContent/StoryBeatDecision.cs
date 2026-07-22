namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatDecision
{
    public string Description { get; set; } = string.Empty;

    public List<StoryBeatDecisionOption> Decisions { get; set; } = [];
}

public class StoryBeatDecisionOption
{
    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}
