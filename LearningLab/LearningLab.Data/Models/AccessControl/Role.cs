namespace LearningLab.Data.Models.AccessControl;

public sealed class Role
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<UserRole> UserRoles { get; set; } = [];

    public List<RolePermission> RolePermissions { get; set; } = [];
}
