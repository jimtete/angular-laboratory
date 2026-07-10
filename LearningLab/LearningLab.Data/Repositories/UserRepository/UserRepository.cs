using LearningLab.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly LearningLabContext _context;

    public UserRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return QueryUsersWithAccessControl()
            .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return QueryUsersWithAccessControl()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AnyAsync(user => user.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<User> QueryUsersWithAccessControl()
    {
        return _context.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission);
    }
}
