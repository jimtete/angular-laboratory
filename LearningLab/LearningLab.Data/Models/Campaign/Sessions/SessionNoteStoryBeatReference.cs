using LearningLab.Data.Models.Campaign.Story;

namespace LearningLab.Data.Models.Campaign.Sessions;

public class SessionNoteStoryBeatReference
{
    public int Id { get; set; }
    public int SessionNoteId { get; set; }
    public SessionNote SessionNote { get; set; } = null!;
    public Guid StoryBeatId { get; set; }
    public StoryBeat StoryBeat { get; set; } = null!;
    public SessionNoteStoryBeatReferenceType ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public SessionNoteStoryBeatReferenceOutcome ReferenceOutcome { get; set; }
    public string? NpcTag { get; set; }
    public string ContentSnapshot { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
