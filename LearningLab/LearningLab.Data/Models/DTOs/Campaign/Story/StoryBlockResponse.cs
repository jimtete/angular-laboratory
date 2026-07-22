namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class StoryBlockResponse
{
    public Guid StoryBlockId { get; init; }

    public Guid CampaignId { get; init; }

    public required string Title { get; init; }
}
