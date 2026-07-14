using Microsoft.AspNetCore.SignalR;
using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.Notifications;

namespace LearningLab.Sockets;

public static class SocketServiceCollectionExtensions
{
    public static IServiceCollection AddLearningLabSockets(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, LearningLabUserIdProvider>();
        services.AddScoped<IApplicationEventHandler<NotificationCreatedEvent>, NotificationCreatedSocketEventHandler>();
        services.AddScoped<IApplicationEventHandler<NotificationDeletedEvent>, NotificationDeletedSocketEventHandler>();

        return services;
    }
}
