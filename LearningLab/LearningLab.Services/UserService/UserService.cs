using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.UserService;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<string>>> GetPlayerUsernamesAsync(
        CancellationToken cancellationToken = default)
    {
        var usernames = await _userRepository.ListUsernamesByRoleAsync(
            AccessRoleNames.Player,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<string>>(
            ApplicationStatusCode.Success,
            usernames);
    }
}
