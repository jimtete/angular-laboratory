using LearningLab.Data.Models.Character;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.CharacterSheetRepository;

public sealed class CharacterSheetRepository : ICharacterSheetRepository
{
    private readonly LearningLabContext _context;

    public CharacterSheetRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<CharacterSheet?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.CharacterSheets
            .SingleOrDefaultAsync(
                characterSheet => characterSheet.UserId == userId,
                cancellationToken);
    }

    public Task<bool> ExistsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.CharacterSheets
            .AnyAsync(
                characterSheet => characterSheet.UserId == userId,
                cancellationToken);
    }

    public async Task AddAsync(
        CharacterSheet characterSheet,
        CancellationToken cancellationToken = default)
    {
        await _context.CharacterSheets.AddAsync(characterSheet, cancellationToken);
    }

    public void Update(CharacterSheet characterSheet)
    {
        _context.CharacterSheets.Update(characterSheet);
    }

    public void Remove(CharacterSheet characterSheet)
    {
        _context.CharacterSheets.Remove(characterSheet);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
