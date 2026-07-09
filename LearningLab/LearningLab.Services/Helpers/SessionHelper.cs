using System.Security.Claims;

namespace LearningLab.Services.Helpers;

public static class SessionHelper
{
    private const string SubjectClaimType = "sub";

    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(SubjectClaimType)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}
