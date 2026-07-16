using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.Campaign;

public class CampaignSettings
{
    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int MaxNumberOfPlayers { get; set; }

    public PassiveSkillsCheck PassiveSkillsCheck { get; set; } = PassiveSkillsCheck.Manual;
    
    [MaxLength(16384)]
    public string CampaignDescription { get; set; } = string.Empty;
}
