using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class UpdateCampaignMemberSkillsRequest
{
    public IReadOnlyList<Skill> HalfProficientSkills { get; init; } = [];

    public IReadOnlyList<Skill> ProficientSkills { get; init; } = [];

    public IReadOnlyList<Skill> ExpertiseSkills { get; init; } = [];
}
