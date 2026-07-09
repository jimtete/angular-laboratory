using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Auth;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.Services.Helpers;

namespace LearningLab.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> RegisterUserAsync(RegisterUserRequest userRequest, CancellationToken cancellationToken = default)
    {
        var usernameExists = await _userRepository.ExistsByUsernameAsync(userRequest.Username, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        var password = AuthHelper.HashPassword(userRequest.Password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = userRequest.Username,
            Password = password.PasswordHash,
            PasswordSalt = password.PasswordSalt,
            FirstName = userRequest.FirstName,
            LastName = userRequest.LastName
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return user.UserId;
    }

    public async Task<Guid> LoginUserAsync(LoginUserRequest loginUserRequest, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(loginUserRequest.Username, cancellationToken);

        if (user is null || !AuthHelper.VerifyPassword(loginUserRequest.Password, user.Password, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return user.UserId;
    }
}
