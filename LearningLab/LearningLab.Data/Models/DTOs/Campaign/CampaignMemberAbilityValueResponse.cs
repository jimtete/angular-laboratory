using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignMemberAbilityValueResponse
{
    public Ability Ability { get; init; }

    public int Value { get; init; }
}
