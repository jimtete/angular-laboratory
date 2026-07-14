namespace LearningLab.Services.Eventing;

public interface IApplicationEventHub
{
    Task PublishAsync<TEvent>(
        TEvent applicationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IApplicationEvent;
}
