using LearningLab.Data.Models.Monsters;

namespace LearningLab.Data.Repositories.MonsterRepository;

public interface IMonsterRepository
{
    Task<IReadOnlyList<Monster>> ListAsync(CancellationToken cancellationToken = default);

    Task<Monster?> GetByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task<Monster?> GetMutableByIdAsync(
        int monsterId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Monster monster,
        CancellationToken cancellationToken = default);

    void Remove(Monster monster);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
