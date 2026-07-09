using LearningLab.Data.Models;

namespace LearningLab.Data.Repositories.UserRepository;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
