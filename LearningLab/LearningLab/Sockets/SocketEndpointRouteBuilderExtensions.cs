namespace LearningLab.Sockets;

public static class SocketEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLearningLabSockets(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationsHub>(SocketEndpointPaths.Notifications);

        return endpoints;
    }
}
