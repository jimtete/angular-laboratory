using LearningLab.Data.Models.DTOs.Auth;

namespace LearningLab.Services.Security;

public interface IJwtTokenGenerator
{
    AuthResponse GenerateToken(Guid userId, string username);
}
