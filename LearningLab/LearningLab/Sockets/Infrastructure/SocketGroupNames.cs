namespace LearningLab.Sockets.Infrastructure;

public static class SocketGroupNames
{
    public static string UserNotifications(Guid userId)
    {
        return $"user:{userId:D}:notifications";
    }

    public static string CampaignSessions(Guid campaignId)
    {
        return $"campaign:{campaignId:D}:sessions";
    }
}
