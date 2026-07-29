namespace LearningLab.Sockets.PresentationMode;

public sealed class PresentationModeSocketErrorResponse
{
    public required string Operation { get; init; }

    public required string ErrorCode { get; init; }

    public required string Message { get; init; }

    public Guid? CampaignId { get; init; }

    public int? SessionId { get; init; }
}
