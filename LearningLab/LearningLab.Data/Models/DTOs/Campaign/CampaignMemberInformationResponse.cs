using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignMemberInformationResponse
{
    public Guid UserId { get; init; }

    public required string Username { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? Nickname { get; init; }

    public IReadOnlyList<Skill> HalfProficientSkills { get; init; } = [];

    public IReadOnlyList<Skill> ProficientSkills { get; init; } = [];

    public IReadOnlyList<Skill> ExpertiseSkills { get; init; } = [];

    public IReadOnlyList<CampaignMemberAbilityValueResponse> AbilityValues { get; init; } = [];

    public IReadOnlyList<CampaignMemberSkillValueResponse> SkillValues { get; init; } = [];
}
