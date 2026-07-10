namespace LearningLab.Data.Models.AccessControl;

public static class AccessRoleNames
{
    public const string Master = "Master";
    public const string Player = "Player";
    public const string MasterOrPlayer = $"{Master},{Player}";
}
