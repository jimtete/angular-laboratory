namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateCampaignNpcRequest
{
    public string? Name { get; init; }

    public string? DisplayName { get; init; }

    public string? Nickname { get; init; }

    public string? Description { get; init; }
}
