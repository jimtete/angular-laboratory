namespace LearningLab.Services.Eventing;

public interface IApplicationEventHandler<in TEvent>
    where TEvent : IApplicationEvent
{
    Task HandleAsync(
        TEvent applicationEvent,
        CancellationToken cancellationToken = default);
}
