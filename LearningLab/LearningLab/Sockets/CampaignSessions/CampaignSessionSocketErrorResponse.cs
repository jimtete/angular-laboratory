namespace LearningLab.Sockets.CampaignSessions;

public sealed class CampaignSessionSocketErrorResponse
{
    public required string Operation { get; init; }

    public required string ErrorCode { get; init; }

    public required string Message { get; init; }

    public Guid? CampaignId { get; init; }
}
