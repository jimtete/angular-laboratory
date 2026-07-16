namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CreateCampaignMilestoneSessionNoteRequest
{
    public int? MilestoneId { get; init; }

    public string? Content { get; init; }

    public CampaignMilestoneRequest? Milestone { get; init; }
}
