namespace LearningLab.Infrastructure.StaticAssets;

public static class StaticAssetUrlExtensions
{
    public static string? ToPublicStaticAssetUrl(
        this HttpRequest request,
        string? assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return assetPath;
        }

        if (Uri.TryCreate(assetPath, UriKind.Absolute, out _))
        {
            return assetPath;
        }

        if (!assetPath.StartsWith(
                LearningLabStaticAssetDefaults.RequestPath + "/",
                StringComparison.OrdinalIgnoreCase))
        {
            return assetPath;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}{assetPath}";
    }
}
