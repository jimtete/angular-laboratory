namespace LearningLab.Services.Configuration;

public sealed class ProfilePictureStorageOptions
{
    public const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;

    public string RootPath { get; set; } = string.Empty;

    public string RequestPath { get; set; } = "/assets";

    public long MaxFileSizeBytes { get; set; } = DefaultMaxFileSizeBytes;
}
