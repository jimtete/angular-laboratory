using LearningLab.Data.Models.Campaign.Rules;
using LearningLab.Data.Models.Campaign.Presentation;

namespace LearningLab.Data.Models.Campaign.Sessions;

public class CampaignSession
{
    public int Id { get; set; }
    public Guid CampaignId { get; set; }
    public int SessionNumber { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? SessionDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public ICollection<SessionNote> Notes { get; set; } = [];
    public ICollection<CampaignEventState> EventStates { get; set; } = [];
    public ICollection<CampaignChoiceSelection> ChoiceSelections { get; set; } = [];
    public CampaignPresentation? Presentation { get; set; }
}
