using LearningLab.Data.Models.Campaign.Sessions;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignSessionRepository;

public sealed class CampaignSessionRepository : ICampaignSessionRepository
{
    private readonly LearningLabContext _context;

    public CampaignSessionRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignSession>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignSessions
            .AsNoTracking()
            .Where(session => session.CampaignId == campaignId)
            .OrderBy(session => session.SessionNumber)
            .ThenBy(session => session.SessionDate)
            .ToListAsync(cancellationToken);
    }

    public Task<int?> GetLatestSessionNumberByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignSessions
            .Where(session => session.CampaignId == campaignId)
            .MaxAsync(
                session => (int?)session.SessionNumber,
                cancellationToken);
    }

    public Task<CampaignSession?> GetByCampaignIdAndSessionIdAsync(
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignSessions
            .SingleOrDefaultAsync(
                session => session.CampaignId == campaignId
                    && session.Id == sessionId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignSession session,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignSessions.AddAsync(session, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
