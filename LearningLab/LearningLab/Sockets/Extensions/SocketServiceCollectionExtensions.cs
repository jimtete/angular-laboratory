using Microsoft.AspNetCore.SignalR;
using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.CampaignSessions;
using LearningLab.Services.Eventing.Notifications;
using LearningLab.Sockets.CampaignSessions;
using LearningLab.Sockets.Infrastructure;
using LearningLab.Sockets.Notifications;

namespace LearningLab.Sockets.Extensions;

public static class SocketServiceCollectionExtensions
{
    public static IServiceCollection AddLearningLabSockets(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        services.AddSingleton<IUserIdProvider, LearningLabUserIdProvider>();
        services.AddScoped<IApplicationEventHandler<CampaignSessionCreatedEvent>, CampaignSessionCreatedSocketEventHandler>();
        services.AddScoped<IApplicationEventHandler<CampaignSessionUpdatedEvent>, CampaignSessionUpdatedSocketEventHandler>();
        services.AddScoped<IApplicationEventHandler<NotificationCreatedEvent>, NotificationCreatedSocketEventHandler>();
        services.AddScoped<IApplicationEventHandler<NotificationDeletedEvent>, NotificationDeletedSocketEventHandler>();

        return services;
    }
}
