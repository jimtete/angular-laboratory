using LearningLab.Data.Models.Campaign.Quests;

namespace LearningLab.Data.Models.DTOs.Campaign.Quests;

public sealed class CampaignQuestResponse
{
    public Guid QuestId { get; init; }

    public Guid CampaignId { get; init; }

    public CampaignQuestType Type { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string GivenBy { get; init; }

    public required string Reward { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public IReadOnlyList<CampaignQuestTaskResponse> Tasks { get; init; } = [];

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
