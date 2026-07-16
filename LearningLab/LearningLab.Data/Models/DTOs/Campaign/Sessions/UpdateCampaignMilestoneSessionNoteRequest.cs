namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class UpdateCampaignMilestoneSessionNoteRequest
{
    public int MilestoneId { get; init; }

    public string? Content { get; init; }

    public CampaignMilestoneRequest? Milestone { get; init; }
}
