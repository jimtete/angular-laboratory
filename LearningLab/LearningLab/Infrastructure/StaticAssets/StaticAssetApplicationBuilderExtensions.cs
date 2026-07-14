using LearningLab.Services.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace LearningLab.Infrastructure.StaticAssets;

public static class StaticAssetApplicationBuilderExtensions
{
    public static IServiceCollection AddLearningLabAssetStorage(
        this IServiceCollection services,
        string assetsRoot)
    {
        services.Configure<ProfilePictureStorageOptions>(options =>
        {
            options.RootPath = assetsRoot;
            options.RequestPath = LearningLabStaticAssetDefaults.RequestPath;
        });

        services.Configure<CampaignPictureStorageOptions>(options =>
        {
            options.RootPath = assetsRoot;
            options.RequestPath = LearningLabStaticAssetDefaults.RequestPath;
        });

        return services;
    }

    public static IApplicationBuilder UseLearningLabStaticAssets(
        this IApplicationBuilder app,
        string assetsRoot)
    {
        Directory.CreateDirectory(assetsRoot);

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(assetsRoot),
            RequestPath = LearningLabStaticAssetDefaults.RequestPath,
            OnPrepareResponse = context =>
            {
                var headers = context.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = LearningLabStaticAssetDefaults.CacheMaxAge,
                    Extensions =
                    {
                        new NameValueHeaderValue("immutable")
                    }
                };

                context.Context.Response.Headers.XContentTypeOptions = "nosniff";
            }
        });
    }
}
