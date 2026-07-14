using LearningLab.Services.Eventing;
using Microsoft.Extensions.DependencyInjection;

namespace LearningLab.Infrastructure.Eventing;

public sealed class ApplicationEventHub : IApplicationEventHub
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationEventHub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(
        TEvent applicationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IApplicationEvent
    {
        var handlers = _serviceProvider.GetServices<IApplicationEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(applicationEvent, cancellationToken);
        }
    }
}
