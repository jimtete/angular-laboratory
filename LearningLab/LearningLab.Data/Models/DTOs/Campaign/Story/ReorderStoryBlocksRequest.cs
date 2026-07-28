namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class ReorderStoryBlocksRequest
{
    public IReadOnlyList<Guid> StoryBlockIds { get; init; } = [];
}
