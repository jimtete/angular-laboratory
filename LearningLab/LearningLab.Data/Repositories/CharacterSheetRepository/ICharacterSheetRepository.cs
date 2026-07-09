using LearningLab.Data.Models.Character;

namespace LearningLab.Data.Repositories.CharacterSheetRepository;

public interface ICharacterSheetRepository
{
    Task<CharacterSheet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CharacterSheet characterSheet,
        CancellationToken cancellationToken = default);

    void Update(CharacterSheet characterSheet);

    void Remove(CharacterSheet characterSheet);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
