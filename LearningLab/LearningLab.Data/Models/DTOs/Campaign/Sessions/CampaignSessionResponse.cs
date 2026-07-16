namespace LearningLab.Data.Models.DTOs.Campaign.Sessions;

public sealed class CampaignSessionResponse
{
    public int Id { get; init; }

    public Guid CampaignId { get; init; }

    public int SessionNumber { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? SessionDate { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public IReadOnlyList<SessionNoteResponse> Notes { get; init; } = [];
}
