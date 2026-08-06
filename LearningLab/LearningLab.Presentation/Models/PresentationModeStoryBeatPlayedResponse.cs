using LearningLab.Data.Models.DTOs.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign.Rules;

namespace LearningLab.Presentation.Models;

public sealed class PresentationModeStoryBeatPlayedResponse
{
    public required PresentationModeWorkspaceResponse Workspace { get; init; }

    public required CampaignSessionResponse Session { get; init; }

    public IReadOnlyList<CampaignEventStateResponse> ChangedEventStates { get; init; } = [];
}
