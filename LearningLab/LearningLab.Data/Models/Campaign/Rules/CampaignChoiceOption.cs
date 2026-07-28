using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignChoiceOption
{
    public Guid Id { get; set; }
    public Guid CampaignChoiceDefinitionId { get; set; }
    public CampaignChoiceDefinition CampaignChoiceDefinition { get; set; } = null!;
    public Guid? StoryBeatId { get; set; }
    public StoryBeat? StoryBeat { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
