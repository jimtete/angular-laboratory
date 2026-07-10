using Microsoft.AspNetCore.Authorization;

namespace LearningLab.Security.AccessPermissions;

public static class AccessPermissionAuthorizationExtensions
{
    public static IServiceCollection AddAccessPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AccessPermissionPolicyNames.CharacterSheetRead,
                policy => policy.RequirePermission(AccessPermissions.CharacterSheetRead));

            options.AddPolicy(
                AccessPermissionPolicyNames.CharacterSheetWrite,
                policy => policy.RequirePermission(AccessPermissions.CharacterSheetWrite));

            options.AddPolicy(
                AccessPermissionPolicyNames.CharacterSheetPortraitWrite,
                policy => policy.RequirePermission(AccessPermissions.CharacterSheetPortraitWrite));
        });

        return services;
    }

    private static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder policy,
        string permission)
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new PermissionRequirement(permission));

        return policy;
    }
}
