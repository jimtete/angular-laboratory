using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatIndexPathRuleResponse
{
    public Guid Id { get; init; }

    public Guid CampaignId { get; init; }

    public Guid StoryBlockId { get; init; }

    public int OrderIndex { get; init; }

    public StoryBeatIndexPathRuleRelationType RelationType { get; init; }

    public bool IsRequired { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}
