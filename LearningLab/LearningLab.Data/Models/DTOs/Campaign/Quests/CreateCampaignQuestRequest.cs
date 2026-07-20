using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class CreateCampaignQuestRequest
{
    public CampaignQuestType Type { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? GivenBy { get; init; }

    public string? Reward { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public IReadOnlyList<CampaignQuestTaskRequest> Tasks { get; init; } = [];
}
