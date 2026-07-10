using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearningLab.Data.Models.DTOs.Auth;
using LearningLab.Security.AccessPermissions;
using LearningLab.Services.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LearningLab.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public AuthResponse GenerateToken(
        Guid userId,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var signingCredentials = CreateSigningCredentials();
        var userIdValue = userId.ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userIdValue),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(role => new Claim(ClaimTypes.Role, role)));

        claims.AddRange(permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(permission => new Claim(
                AccessPermissionClaimTypes.Permission,
                permission)));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }
}
