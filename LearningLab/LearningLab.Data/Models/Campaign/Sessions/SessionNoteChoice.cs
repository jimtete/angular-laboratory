namespace LearningLab.Data.Models.Campaign.Sessions;

public class SessionNoteChoice
{
    public int Id { get; set; }
    public int SessionNoteId { get; set; }
    public int Order { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public bool IsChosen { get; set; }
    public SessionNote SessionNote { get; set; } = null!;
}
