namespace LearningLab.Sockets;

public static class SocketGroupNames
{
    public static string UserNotifications(Guid userId)
    {
        return $"user:{userId:D}:notifications";
    }
}
