using LearningLab.Sockets.CampaignSessions;
using LearningLab.Sockets.Infrastructure;
using LearningLab.Sockets.Notifications;

namespace LearningLab.Sockets.Extensions;

public static class SocketEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLearningLabSockets(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationsHub>(SocketEndpointPaths.Notifications);
        endpoints.MapHub<CampaignSessionsHub>(SocketEndpointPaths.CampaignSessions);

        return endpoints;
    }
}
