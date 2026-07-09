using LearningLab.Data.Models.DTOs.Auth;

namespace LearningLab.Services.AuthService;

public interface IAuthService
{
    Task<Guid> RegisterUserAsync(RegisterUserRequest userRequest, CancellationToken cancellationToken = default);

    Task<Guid> LoginUserAsync(LoginUserRequest loginUserRequest, CancellationToken cancellationToken = default);
}
