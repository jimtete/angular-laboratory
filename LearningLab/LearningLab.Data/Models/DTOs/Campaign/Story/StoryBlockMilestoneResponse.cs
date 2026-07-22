using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBlockMilestoneResponse
{
    public Guid StoryBlockId { get; init; }

    public int CampaignMilestoneId { get; init; }

    public int OrderIndex { get; init; }

    public required CampaignMilestoneResponse Milestone { get; init; }
}
