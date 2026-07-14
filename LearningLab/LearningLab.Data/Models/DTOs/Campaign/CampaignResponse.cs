namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignResponse
{
    public Guid CampaignId { get; init; }

    public Guid GameMasterId { get; init; }

    public required string GameMasterUsername { get; init; }

    public required string CampaignName { get; init; }

    public required string Version { get; init; }

    public string? CampaignPictureUrl { get; init; }

    public DateTimeOffset DateCreated { get; init; }
}
