using LearningLab.Data.Models.Assets;

namespace LearningLab.Data.Models.Campaign.Story;

public sealed class StoryBlockMusicFile
{
    public Guid Id { get; set; }

    public Guid StoryBlockId { get; set; }

    public StoryBlock StoryBlock { get; set; } = null!;

    public Guid? StoryBeatId { get; set; }

    public StoryBeat? StoryBeat { get; set; }

    public int MusicFileId { get; set; }

    public MusicFile MusicFile { get; set; } = null!;

    public int OrderIndex { get; set; }

    public bool Loop { get; set; }

    public bool ContinueAcrossStoryBlocks { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
