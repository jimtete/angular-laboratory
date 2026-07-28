using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.Monsters;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.MonsterRepository;

public sealed class MonsterRepository : IMonsterRepository
{
    private readonly LearningLabContext _context;

    public MonsterRepository(LearningLabContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Monster>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Monsters
            .AsNoTracking()
            .OrderBy(monster => monster.Name)
            .ThenBy(monster => monster.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Monster>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CampaignNpcParticipations
            .AsNoTracking()
            .Where(participation => participation.CampaignId == campaignId)
            .Select(participation => participation.Monster)
            .OrderBy(monster => monster.Name)
            .ThenBy(monster => monster.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Monster>> ListDetailedByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await BuildDetailedQuery()
            .AsNoTracking()
            .Where(monster => monster.CampaignParticipations.Any(
                participation => participation.CampaignId == campaignId))
            .OrderBy(monster => monster.Name)
            .ThenBy(monster => monster.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Monster?> GetByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        return BuildDetailedQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                monster => monster.Id == monsterId,
                cancellationToken);
    }

    public Task<Monster?> GetMutableByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        return BuildDetailedQuery()
            .SingleOrDefaultAsync(
                monster => monster.Id == monsterId,
                cancellationToken);
    }

    public Task<bool> ExistsByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        return _context.Monsters
            .AsNoTracking()
            .AnyAsync(
                monster => monster.Id == monsterId,
                cancellationToken);
    }

    public Task<bool> CampaignParticipationExistsAsync(
        Guid campaignId,
        int monsterId,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignNpcParticipations
            .AsNoTracking()
            .AnyAsync(
                participation => participation.CampaignId == campaignId
                    && participation.MonsterId == monsterId,
                cancellationToken);
    }

    public Task<int> CountCampaignParticipationsByMonsterIdsAsync(
        Guid campaignId,
        IReadOnlyCollection<int> monsterIds,
        CancellationToken cancellationToken = default)
    {
        return _context.CampaignNpcParticipations
            .AsNoTracking()
            .CountAsync(
                participation => participation.CampaignId == campaignId
                    && monsterIds.Contains(participation.MonsterId),
                cancellationToken);
    }

    public async Task AddAsync(
        Monster monster,
        CancellationToken cancellationToken = default)
    {
        await _context.Monsters.AddAsync(monster, cancellationToken);
    }

    public async Task AddCampaignParticipationAsync(
        CampaignNpcParticipation participation,
        CancellationToken cancellationToken = default)
    {
        await _context.CampaignNpcParticipations.AddAsync(participation, cancellationToken);
    }

    public void Remove(Monster monster)
    {
        _context.Monsters.Remove(monster);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Monster> BuildDetailedQuery()
    {
        return _context.Monsters
            .Include(monster => monster.Abilities)
            .Include(monster => monster.Proficiencies)
            .Include(monster => monster.Features)
            .Include(monster => monster.Spellcasting)
                .ThenInclude(spellcasting => spellcasting!.SpellSlots);
    }
}
