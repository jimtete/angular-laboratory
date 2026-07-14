using LearningLab.Services.Eventing;

namespace LearningLab.Infrastructure.Eventing;

public static class EventingServiceCollectionExtensions
{
    public static IServiceCollection AddLearningLabEventHub(this IServiceCollection services)
    {
        services.AddScoped<IApplicationEventHub, ApplicationEventHub>();

        return services;
    }
}
