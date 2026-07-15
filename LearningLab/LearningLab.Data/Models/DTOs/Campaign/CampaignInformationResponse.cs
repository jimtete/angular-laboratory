namespace LearningLab.Data.Models.DTOs.Campaign;

public sealed class CampaignInformationResponse
{
    public Guid CampaignId { get; init; }

    public IReadOnlyList<CampaignMemberInformationResponse> JoinedMembers { get; init; } = [];
}
