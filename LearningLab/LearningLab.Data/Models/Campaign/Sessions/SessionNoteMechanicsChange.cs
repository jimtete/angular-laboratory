using LearningLab.Data.Models;

namespace LearningLab.Data.Models.Campaign.Sessions;

public class SessionNoteMechanicsChange
{
    public int Id { get; set; }
    public int SessionNoteId { get; set; }
    public int Order { get; set; }
    public Guid PlayerId { get; set; }
    public string? ChangeText { get; set; }
    public SessionNote SessionNote { get; set; } = null!;
    public User Player { get; set; } = null!;
}
