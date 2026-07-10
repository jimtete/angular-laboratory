using Microsoft.AspNetCore.Authorization;

namespace LearningLab.Security.AccessPermissions;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string policy)
    {
        Policy = policy;
    }
}
