using LearningLab.Data.Models;

namespace LearningLab.Presentation.Actions;

public abstract class PresentAction<TResult>
{
    public abstract Task<ServiceResult<TResult>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);
}

public abstract class PresentAction<TRequest, TResult>
{
    public abstract Task<ServiceResult<TResult>> ExecuteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        TRequest? request,
        CancellationToken cancellationToken = default);
}
