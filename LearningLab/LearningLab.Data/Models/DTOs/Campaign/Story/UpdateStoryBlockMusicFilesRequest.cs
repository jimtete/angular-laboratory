namespace LearningLab.Data.Models.DTOs.Campaign.Story;

public sealed class UpdateStoryBlockMusicFilesRequest
{
    public IReadOnlyList<StoryBlockMusicFileRequest>? MusicFiles { get; init; }
}

public sealed class StoryBlockMusicFileRequest
{
    public int MusicFileId { get; init; }

    public Guid? StoryBeatId { get; init; }

    public int? OrderIndex { get; init; }

    public bool? Loop { get; init; }

    public bool? ContinueAcrossStoryBlocks { get; init; }
}
