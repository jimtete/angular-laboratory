namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatIndexPathRule
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public LearningLab.Data.Models.Campaign.Campaign Campaign { get; set; } = null!;

    public Guid StoryBlockId { get; set; }

    public StoryBlock StoryBlock { get; set; } = null!;

    public int OrderIndex { get; set; }

    public StoryBeatIndexPathRuleRelationType RelationType { get; set; }

    public bool IsRequired { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
