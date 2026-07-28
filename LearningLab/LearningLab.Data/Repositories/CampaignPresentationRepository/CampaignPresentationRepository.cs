using LearningLab.Data.Models.Campaign.Presentation;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignPresentationRepository;

public sealed class CampaignPresentationRepository : ICampaignPresentationRepository
{
    private readonly LearningLabContext _context;

    public CampaignPresentationRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<CampaignPresentation?> GetByCampaignSessionIdAsync(
        int campaignSessionId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignPresentations
            .SingleOrDefaultAsync(
                presentation => presentation.CampaignSessionId == campaignSessionId,
                cancellationToken);
    }

    public Task<CampaignPresentationEntry?> GetLatestEntryAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignPresentationEntries
            .AsNoTracking()
            .Where(entry => entry.CampaignPresentationId == campaignPresentationId)
            .OrderByDescending(entry => entry.Sequence)
            .ThenByDescending(entry => entry.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignPresentationStoryBeatSelection>> ListStoryBeatSelectionsAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignPresentationStoryBeatSelections
            .AsNoTracking()
            .Where(selection => selection.CampaignPresentationId == campaignPresentationId)
            .OrderBy(selection => selection.StoryBlockId)
            .ThenBy(selection => selection.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignPresentationStoryBeatSelection?> GetStoryBeatSelectionAsync(
        int campaignPresentationId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignPresentationStoryBeatSelections
            .SingleOrDefaultAsync(
                selection => selection.CampaignPresentationId == campaignPresentationId
                    && selection.StoryBlockId == storyBlockId
                    && selection.OrderIndex == orderIndex,
                cancellationToken);
    }

    public Task<int?> GetLatestEntrySequenceAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignPresentationEntries
            .AsNoTracking()
            .Where(entry => entry.CampaignPresentationId == campaignPresentationId)
            .MaxAsync(
                entry => (int?)entry.Sequence,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignPresentation presentation,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignPresentations.AddAsync(presentation, cancellationToken);
    }

    public async Task AddEntryAsync(
        CampaignPresentationEntry entry,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignPresentationEntries.AddAsync(entry, cancellationToken);
    }

    public async Task AddStoryBeatSelectionAsync(
        CampaignPresentationStoryBeatSelection selection,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignPresentationStoryBeatSelections.AddAsync(selection, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
