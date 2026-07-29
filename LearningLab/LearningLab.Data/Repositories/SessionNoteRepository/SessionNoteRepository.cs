using LearningLab.Data.Models.Campaign.Sessions;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.SessionNoteRepository;

public sealed class SessionNoteRepository : ISessionNoteRepository
{
    private readonly LearningLabContext _context;

    public SessionNoteRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SessionNote>> ListBySessionIdsAsync(
        IReadOnlyCollection<int> sessionIds,
        CancellationToken cancellationToken = default)
    {
        if (sessionIds.Count == 0)
        {
            return [];
        }

        return await _context.SessionNotes
            .AsNoTracking()
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .Where(note => sessionIds.Contains(note.SessionId))
            .OrderBy(note => note.SessionId)
            .ThenBy(note => note.Order)
            .ThenBy(note => note.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SessionNote>> ListBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SessionNotes
            .AsNoTracking()
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .Where(note => note.SessionId == sessionId)
            .OrderBy(note => note.Order)
            .ThenBy(note => note.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<SessionNote?> GetBySessionIdAndNoteIdAsync(
        int sessionId,
        int noteId,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .SingleOrDefaultAsync(
                note => note.SessionId == sessionId
                    && note.Id == noteId,
                cancellationToken);
    }

    public Task<SessionNote?> GetBySessionIdAndStoryBeatReferenceAsync(
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        Guid? referenceId,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .Where(note => note.SessionId == sessionId
                && note.StoryBeatId == storyBeatId
                && note.StoryBeatReferences.Any(reference =>
                    reference.StoryBeatId == storyBeatId
                    && reference.ReferenceType == referenceType
                    && reference.ReferenceId == referenceId))
            .OrderBy(note => note.Order)
            .ThenBy(note => note.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<SessionNote?> GetBySessionIdAndStoryBeatReferenceTypeAsync(
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .Where(note => note.SessionId == sessionId
                && note.StoryBeatId == storyBeatId
                && note.StoryBeatReferences.Any(reference =>
                    reference.StoryBeatId == storyBeatId
                    && reference.ReferenceType == referenceType))
            .OrderBy(note => note.Order)
            .ThenBy(note => note.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<SessionNote?> GetBySessionIdAndFullStoryBeatAsync(
        int sessionId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .Include(note => note.Choices)
            .Include(note => note.MechanicsChanges)
            .Include(note => note.StoryBeat)
            .Include(note => note.StoryBeatReferences)
            .Where(note => note.SessionId == sessionId
                && note.StoryBeatId == storyBeatId
                && !note.StoryBeatReferences.Any())
            .OrderBy(note => note.Order)
            .ThenBy(note => note.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int?> GetLatestOrderBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .AsNoTracking()
            .Where(note => note.SessionId == sessionId)
            .MaxAsync(
                note => (int?)note.Order,
                cancellationToken);
    }

    public Task DecrementOrderAfterAsync(
        int sessionId,
        int order,
        CancellationToken cancellationToken = default)
    {
        return _context.SessionNotes
            .Where(note => note.SessionId == sessionId && note.Order > order)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    note => note.Order,
                    note => note.Order - 1),
                cancellationToken);
    }

    public async Task AddAsync(
        SessionNote note,
        CancellationToken cancellationToken = default)
    {
        await _context.SessionNotes.AddAsync(note, cancellationToken);
    }

    public void Remove(SessionNote note)
    {
        _context.SessionNotes.Remove(note);
    }

    public void RemoveStoryBeatReference(SessionNoteStoryBeatReference reference)
    {
        _context.SessionNoteStoryBeatReferences.Remove(reference);
    }
}
