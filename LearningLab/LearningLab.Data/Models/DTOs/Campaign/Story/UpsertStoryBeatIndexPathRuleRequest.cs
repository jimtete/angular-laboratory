using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpsertStoryBeatIndexPathRuleRequest
{
    public StoryBeatIndexPathRuleRelationType RelationType { get; init; }

    public bool IsRequired { get; init; }
}
