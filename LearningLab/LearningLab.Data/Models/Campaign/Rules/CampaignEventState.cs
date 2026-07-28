using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Rules;

public sealed class CampaignEventState
{
    public Guid Id { get; set; }
    public int CampaignSessionId { get; set; }
    public CampaignSession CampaignSession { get; set; } = null!;
    public Guid CampaignEventDefinitionId { get; set; }
    public CampaignEventDefinition CampaignEventDefinition { get; set; } = null!;
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public CampaignEventOption? SelectedOption { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public Guid? SourceStoryBlockId { get; set; }
    public StoryBlock? SourceStoryBlock { get; set; }
    public Guid? SourceStoryBeatId { get; set; }
    public StoryBeat? SourceStoryBeat { get; set; }
    public DateTimeOffset ResolvedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
