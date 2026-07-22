namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CampaignNpcResponse
{
    public Guid CampaignNpcId { get; init; }

    public Guid CampaignId { get; init; }

    public required string Tag { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string Description { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
