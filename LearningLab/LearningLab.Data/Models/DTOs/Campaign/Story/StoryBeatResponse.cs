using LearningLab.Data.Models.Campaign.Story;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBeatResponse
{
    public Guid StoryBeatId { get; init; }

    public Guid StoryBlockId { get; init; }

    public int OrderIndex { get; init; }

    public required string Title { get; init; }

    public StoryBeatType StoryBeatType { get; init; }

    public StoryBeatInformationResponse? Information { get; init; }

    public StoryBeatNarrativeResponse? Narrative { get; init; }

    public StoryBeatRoleplayingResponse? Roleplaying { get; init; }

    public StoryBeatDecisionResponse? Decision { get; init; }

    public CampaignMilestoneResponse? Milestone { get; init; }
}
