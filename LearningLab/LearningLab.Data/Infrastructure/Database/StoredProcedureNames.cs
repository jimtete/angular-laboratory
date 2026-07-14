namespace LearningLab.Data.Infrastructure.Database;

public static class StoredProcedureNames
{
    public static class Platform
    {
        public const string GetCampaignsByGameMasterId = "platform.GetCampaignsByGameMasterId";
        public const string CreateNotification = "platform.CreateNotification";
        public const string GetAvailableNotificationsByUserId = "platform.GetAvailableNotificationsByUserId";
    }
}
