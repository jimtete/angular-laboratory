using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignChoiceDefinition
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
    public Guid? StoryBlockId { get; set; }
    public StoryBlock? StoryBlock { get; set; }
    public Guid? StoryBeatId { get; set; }
    public StoryBeat? StoryBeat { get; set; }
    public string Name { get; set; } = string.Empty;
    public CampaignChoiceSelectionMode SelectionMode { get; set; }
    public ICollection<CampaignChoiceOption> Options { get; set; } = [];
}
