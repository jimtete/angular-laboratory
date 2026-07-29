using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Presentation.Models;

public sealed class PresentationModeStoryBlockResponse
{
    public required StoryBlockResponse StoryBlock { get; init; }

    public IReadOnlyList<StoryBeatResponse> StoryBeats { get; init; } = [];

    public IReadOnlyList<CampaignQuestResponse> Quests { get; init; } = [];

    public IReadOnlyList<StoryBeatQuestTaskResponse> StoryBeatQuestTaskLinks { get; init; } = [];
}
