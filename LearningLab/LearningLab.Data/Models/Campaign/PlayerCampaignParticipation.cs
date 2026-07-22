using LearningLab.Data.Models;
using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Models.Campaign;

public class PlayerCampaignParticipation
{
    public Guid CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string? Nickname { get; set; }

    public List<Skill> HalfProficientSkills { get; set; } = [];

    public List<Skill> ProficientSkills { get; set; } = [];

    public List<Skill> ExpertiseSkills { get; set; } = [];

    public List<PlayerCampaignParticipationAbilityValue> AbilityValues { get; set; } = [];

    public List<PlayerCampaignParticipationSkillValue> SkillValues { get; set; } = [];

    public DateTimeOffset DateJoined { get; set; }
}

public class PlayerCampaignParticipationAbilityValue
{
    public Ability Ability { get; set; }

    public int Value { get; set; }
}

public class PlayerCampaignParticipationSkillValue
{
    public Skill Skill { get; set; }

    public int Value { get; set; }
}
