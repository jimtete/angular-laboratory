using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignMemberSkillValueResponse
{
    public Skill Skill { get; init; }

    public int Value { get; init; }
}
