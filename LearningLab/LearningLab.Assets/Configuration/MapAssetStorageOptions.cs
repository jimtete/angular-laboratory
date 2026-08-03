namespace LearningLab.Assets.Configuration;

public sealed class MapAssetStorageOptions
{
    public string RootPath { get; set; } = string.Empty;

    public string RequestPath { get; set; } = "/assets";

    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
}
