using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Presentation.Models;

public sealed class PresentationModeStoryBeatPlayedResponse
{
    public required PresentationModeWorkspaceResponse Workspace { get; init; }

    public required CampaignSessionResponse Session { get; init; }
}
