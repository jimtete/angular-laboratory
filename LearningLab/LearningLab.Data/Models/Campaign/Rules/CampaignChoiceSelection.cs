using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignChoiceSelection
{
    public Guid Id { get; set; }
    public int CampaignSessionId { get; set; }
    public CampaignSession CampaignSession { get; set; } = null!;
    public Guid CampaignChoiceDefinitionId { get; set; }
    public CampaignChoiceDefinition CampaignChoiceDefinition { get; set; } = null!;
    public Guid CampaignChoiceOptionId { get; set; }
    public CampaignChoiceOption CampaignChoiceOption { get; set; } = null!;
    public DateTimeOffset SelectedAtUtc { get; set; }
}
