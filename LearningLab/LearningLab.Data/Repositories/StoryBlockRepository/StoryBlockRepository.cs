using LearningLab.Data.Models.Campaign.Story;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.StoryBlockRepository;

public sealed class StoryBlockRepository : IStoryBlockRepository
{
    private readonly LearningLabContext _context;

    public StoryBlockRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StoryBlock>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoryBlocks
            .AsNoTracking()
            .Where(block => block.CampaignId == campaignId)
            .OrderBy(block => block.StoryBlockId)
            .ToListAsync(cancellationToken);
    }

    public Task<StoryBlock?> GetByCampaignIdAndStoryBlockIdAsync(
        Guid campaignId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default)
    {
        return _context.StoryBlocks
            .SingleOrDefaultAsync(
                block => block.CampaignId == campaignId
                    && block.StoryBlockId == storyBlockId,
                cancellationToken);
    }

    public async Task AddAsync(
        StoryBlock storyBlock,
        CancellationToken cancellationToken = default)
    {
        await _context.StoryBlocks.AddAsync(storyBlock, cancellationToken);
    }

    public void Update(StoryBlock storyBlock)
    {
        _context.StoryBlocks.Update(storyBlock);
    }

    public void Remove(StoryBlock storyBlock)
    {
        _context.StoryBlocks.Remove(storyBlock);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

}
