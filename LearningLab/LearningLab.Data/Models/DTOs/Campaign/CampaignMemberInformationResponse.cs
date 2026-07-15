namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignMemberInformationResponse
{
    public required string Username { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? Nickname { get; init; }
}
