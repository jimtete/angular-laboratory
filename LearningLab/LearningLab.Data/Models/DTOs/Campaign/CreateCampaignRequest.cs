using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CreateCampaignRequest
{
    [Required]
    public required string CampaignName { get; init; }

    [Required]
    public required string Version { get; init; }
}
