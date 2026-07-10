using LearningLab.Data.Models.AccessControl;
using Microsoft.EntityFrameworkCore;

namespace LearningLab.Data.Repositories.RoleRepository;

public sealed class RoleRepository : IRoleRepository
{
    private readonly LearningLabContext _context;

    public RoleRepository(LearningLabContext context)
    {
        _context = context;
    }

    public Task<Role?> GetByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return _context.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(
                role => role.Name == roleName,
                cancellationToken);
    }
}
