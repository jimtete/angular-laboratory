namespace LearningLab.Data.Models.Campaign.Sessions;

public class SessionNote
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int Order { get; set; }
    public SessionNoteType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public CampaignSession Session { get; set; } = null!;
    public ICollection<SessionNoteChoice> Choices { get; set; } = [];
}
