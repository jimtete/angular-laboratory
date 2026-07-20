namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignUsernamesResponse
{
    public Guid CampaignId { get; set; }

    public IReadOnlyList<CampaignMemberInformationResponse> JoinedMembers { get; set; } = [];

    public IReadOnlyList<string> JoinedUsernames { get; set; } = [];

    public IReadOnlyList<string> InvitedUsernames { get; set; } = [];
}
