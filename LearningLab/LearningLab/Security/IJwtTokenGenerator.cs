using System.Security.Claims;
using LearningLab.Data.Models.DTOs.Auth;

namespace LearningLab.Security;

public interface IJwtTokenGenerator
{
    AuthResponse GenerateToken(
        string userId,
        string email,
        IEnumerable<Claim>? additionalClaims = null);
}
