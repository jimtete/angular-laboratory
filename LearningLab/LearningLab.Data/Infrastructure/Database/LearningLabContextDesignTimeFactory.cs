using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LearningLab.Data.Infrastructure.Database;

public sealed class LearningLabContextDesignTimeFactory : IDesignTimeDbContextFactory<LearningLabContext>
{
    private const string DefaultConnectionName = "DefaultConnection";

    public LearningLabContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();
        var options = new DbContextOptionsBuilder<LearningLabContext>()
            .UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .Options;

        return new LearningLabContext(options);
    }

    private static string ResolveConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable(
                $"ConnectionStrings__{DefaultConnectionName}")
            ?? Environment.GetEnvironmentVariable(DefaultConnectionName);

        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var appSettingsPath = FindAppSettingsPath();
        using var stream = File.OpenRead(appSettingsPath);
        using var document = JsonDocument.Parse(stream);

        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.TryGetProperty(DefaultConnectionName, out var connectionString)
            && !string.IsNullOrWhiteSpace(connectionString.GetString()))
        {
            return connectionString.GetString()!;
        }

        throw new InvalidOperationException(
            $"Connection string '{DefaultConnectionName}' was not found in environment variables or {appSettingsPath}.");
    }

    private static string FindAppSettingsPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "LearningLab",
                "appsettings.Development.json");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not find LearningLab/appsettings.Development.json from the current working directory.");
    }
}
