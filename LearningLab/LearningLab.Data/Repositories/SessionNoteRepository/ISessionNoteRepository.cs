using LearningLab.Data.Models.Campaign.Sessions;

namespace LearningLab.Data.Repositories.SessionNoteRepository;

public interface ISessionNoteRepository
{
    Task<IReadOnlyList<SessionNote>> ListBySessionIdsAsync(
        IReadOnlyCollection<int> sessionIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionNote>> ListBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<SessionNote?> GetBySessionIdAndNoteIdAsync(
        int sessionId,
        int noteId,
        CancellationToken cancellationToken = default);

    Task<SessionNote?> GetBySessionIdAndStoryBeatReferenceAsync(
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        Guid? referenceId,
        CancellationToken cancellationToken = default);

    Task<SessionNote?> GetBySessionIdAndStoryBeatReferenceTypeAsync(
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        CancellationToken cancellationToken = default);

    Task<SessionNote?> GetBySessionIdAndFullStoryBeatAsync(
        int sessionId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default);

    Task<int?> GetLatestOrderBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task DecrementOrderAfterAsync(
        int sessionId,
        int order,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SessionNote note,
        CancellationToken cancellationToken = default);

    void Remove(SessionNote note);

    void RemoveStoryBeatReference(SessionNoteStoryBeatReference reference);
}
