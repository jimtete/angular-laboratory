namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignPendingInviteResponse
{
    public Guid CampaignId { get; set; }

    public string CampaignName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string? CampaignPictureUrl { get; set; }

    public Guid GameMasterId { get; set; }

    public string GameMasterUsername { get; set; } = string.Empty;

    public DateTimeOffset DateInvited { get; set; }
}
