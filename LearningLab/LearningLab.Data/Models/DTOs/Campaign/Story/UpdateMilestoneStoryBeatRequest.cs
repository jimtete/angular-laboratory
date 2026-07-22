namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateMilestoneStoryBeatRequest
{
    public string? Title { get; init; }

    public int MilestoneId { get; init; }
}
