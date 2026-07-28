namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class CreateMilestoneStoryBeatRequest
{
    public string? Title { get; init; }

    public int? OrderIndex { get; init; }

    public int? SecondaryOrderIndex { get; init; }

    public int MilestoneId { get; init; }
}
