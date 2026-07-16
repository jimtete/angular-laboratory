using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignSettingsResponse
{
    public Guid CampaignId { get; init; }

    public int MaxNumberOfPlayers { get; init; }

    public PassiveSkillsCheck PassiveSkillsCheck { get; init; }
}
