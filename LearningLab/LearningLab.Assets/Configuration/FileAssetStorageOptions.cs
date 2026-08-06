namespace LearningLab.Assets.Configuration;

public sealed class FileAssetStorageOptions
{
    public string RootPath { get; set; } = string.Empty;

    public string RequestPath { get; set; } = "/assets";

    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
}
