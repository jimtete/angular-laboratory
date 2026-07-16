using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CampaignParticipationInviteRepository;

public sealed class CampaignParticipationInviteRepository : ICampaignParticipationInviteRepository
{
    private readonly LearningLabContext _context;

    public CampaignParticipationInviteRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignParticipationInvite>> ListPendingByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignParticipationInvites
            .Include(invite => invite.Campaign)
            .ThenInclude(campaign => campaign.GameMaster)
            .Where(invite => invite.UserId == userId)
            .OrderByDescending(invite => invite.DateInvited)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListParticipantUsernamesByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlayerCampaignParticipations
            .Where(participation => participation.CampaignId == campaignId)
            .OrderBy(participation => participation.User.Username)
            .Select(participation => participation.User.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignMemberInformationResponse>> ListParticipantInformationByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlayerCampaignParticipations
            .Where(participation => participation.CampaignId == campaignId)
            .OrderBy(participation => participation.User.Username)
            .Select(participation => new CampaignMemberInformationResponse
            {
                Username = participation.User.Username,
                FirstName = participation.User.FirstName,
                LastName = participation.User.LastName,
                Nickname = participation.Nickname,
                HalfProficientSkills = participation.HalfProficientSkills,
                ProficientSkills = participation.ProficientSkills,
                ExpertiseSkills = participation.ExpertiseSkills
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListInviteUsernamesByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignParticipationInvites
            .Where(invite => invite.CampaignId == campaignId)
            .OrderBy(invite => invite.User.Username)
            .Select(invite => invite.User.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountReservedPlayerSlotsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var participantCount = await _context.PlayerCampaignParticipations
            .CountAsync(
                participation => participation.CampaignId == campaignId,
                cancellationToken);

        var inviteCount = await _context.CampaignParticipationInvites
            .CountAsync(
                invite => invite.CampaignId == campaignId,
                cancellationToken);

        return participantCount + inviteCount;
    }

    public Task<CampaignParticipationInvite?> GetInviteAsync(
        Guid campaignId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignParticipationInvites
            .SingleOrDefaultAsync(
                invite => invite.CampaignId == campaignId
                    && invite.UserId == userId,
                cancellationToken);
    }

    public Task<PlayerCampaignParticipation?> GetParticipationByUsernameAsync(
        Guid campaignId,
        string username,
        CancellationToken cancellationToken = default)
    {
        return _context.PlayerCampaignParticipations
            .Include(participation => participation.User)
            .SingleOrDefaultAsync(
                participation => participation.CampaignId == campaignId
                    && participation.User.Username == username,
                cancellationToken);
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

    public async Task AddParticipationAsync(
        PlayerCampaignParticipation participation,
        CancellationToken cancellationToken = default)
    {
        await _context.PlayerCampaignParticipations.AddAsync(participation, cancellationToken);
    }

    public void RemoveInvite(CampaignParticipationInvite invite)
    {
        _context.CampaignParticipationInvites.Remove(invite);
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
