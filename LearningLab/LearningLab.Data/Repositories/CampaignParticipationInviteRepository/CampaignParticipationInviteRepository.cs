using LearningLab.Data.Models.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignParticipationInviteRepository;

public sealed class CampaignParticipationInviteRepository : ICampaignParticipationInviteRepository
{
    private readonly LearningLabContext _context;

    public CampaignParticipationInviteRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsInviteAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignParticipationInvites
            .AnyAsync(
                invite => invite.CampaignId == campaignId
                    && invite.UserId == userId,
                cancellationToken);
    }

    public Task<bool> ExistsParticipationAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.PlayerCampaignParticipations
            .AnyAsync(
                participation => participation.CampaignId == campaignId
                    && participation.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
        CampaignParticipationInvite invite,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignParticipationInvites.AddAsync(invite, cancellationToken);
    }

    public Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            await operation();
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
