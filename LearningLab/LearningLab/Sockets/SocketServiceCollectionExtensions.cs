using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets;

public static class SocketServiceCollectionExtensions
{
    public static IServiceCollection AddLearningLabSockets(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, LearningLabUserIdProvider>();

        return services;
    }
}
