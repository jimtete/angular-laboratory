using LearningLab.Data.Models;

namespace LearningLab.Services.UserService;

public interface IUserService
{
    Task<ServiceResult<IReadOnlyList<string>>> GetPlayerUsernamesAsync(
        CancellationToken cancellationToken = default);
}
