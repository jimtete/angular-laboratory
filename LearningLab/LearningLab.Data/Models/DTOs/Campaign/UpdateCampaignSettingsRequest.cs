using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class UpdateCampaignSettingsRequest
{
    [Range(1, int.MaxValue)]
    public int MaxNumberOfPlayers { get; init; }
    
    [MaxLength(16384)]
    public string CampaignDescription { get; set; } = string.Empty;
}
