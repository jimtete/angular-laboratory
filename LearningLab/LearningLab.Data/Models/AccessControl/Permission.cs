namespace LearningLab.Data.Models.AccessControl;

public sealed class Permission
{
    public Guid PermissionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<RolePermission> RolePermissions { get; set; } = [];
}
