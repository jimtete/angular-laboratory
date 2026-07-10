using LearningLab.Data.Models.AccessControl;

namespace LearningLab.Data.Repositories.RoleRepository;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default);
}
