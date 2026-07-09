using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Auth;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.Services.Helpers;
using LearningLab.Services.Security;

namespace LearningLab.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterUserAsync(
        RegisterUserRequest userRequest,
        CancellationToken cancellationToken = default)
    {
        var usernameExists = await _userRepository.ExistsByUsernameAsync(userRequest.Username, cancellationToken);

        if (usernameExists)
        {
            return new ServiceResult<AuthResponse>(ApplicationStatusCode.UsernameAlreadyExists);
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

        var authResponse = _jwtTokenGenerator.GenerateToken(user.UserId, user.Username);

        return new ServiceResult<AuthResponse>(ApplicationStatusCode.Success, authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> LoginUserAsync(
        LoginUserRequest loginUserRequest,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(loginUserRequest.Username, cancellationToken);

        if (user is null || !AuthHelper.VerifyPassword(loginUserRequest.Password, user.Password, user.PasswordSalt))
        {
            return new ServiceResult<AuthResponse>(ApplicationStatusCode.InvalidCredentials);
        }

        var authResponse = _jwtTokenGenerator.GenerateToken(user.UserId, user.Username);

        return new ServiceResult<AuthResponse>(ApplicationStatusCode.Success, authResponse);
    }
}
