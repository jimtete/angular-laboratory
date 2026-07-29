using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Data.Models.DTOs.Campaign.Quests;

namespace LearningLab.Presentation.Models;

public sealed class PresentationModeWorkspaceResponse
{
    public required CampaignPresentationResponse Presentation { get; init; }

    public IReadOnlyList<PresentationModeStoryBlockResponse> StoryBlocks { get; init; } = [];

    public IReadOnlyList<CampaignQuestResponse> Quests { get; init; } = [];

    public IReadOnlyList<StoryBeatQuestTaskResponse> StoryBeatQuestTaskLinks { get; init; } = [];
}
