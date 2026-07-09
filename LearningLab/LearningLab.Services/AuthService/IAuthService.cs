using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Auth;

namespace LearningLab.Services.AuthService;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterUserAsync(
        RegisterUserRequest userRequest,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AuthResponse>> LoginUserAsync(
        LoginUserRequest loginUserRequest,
        CancellationToken cancellationToken = default);
}
