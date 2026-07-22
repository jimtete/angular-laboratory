namespace LearningLab.Data.Models.Campaign.Story;

public class StoryBeatNarrative
{
    public List<StoryBeatNarrativeParagraph> Paragraphs { get; set; } = [];
}

public class StoryBeatNarrativeParagraph
{
    public int OrderIndex { get; set; }

    public string Text { get; set; } = string.Empty;
}
