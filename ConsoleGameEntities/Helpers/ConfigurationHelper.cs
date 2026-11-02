using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConsoleGameEntities.Helpers;

public static class ConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration(
        string? basePath = null,
        string? environmentName = null)
    {
        basePath ??= AppContext.BaseDirectory;

        environmentName ??=
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{environmentName}.json",
                optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    public static void ConfigureDbContextOptions(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString)
            .UseLazyLoadingProxies();
    }
}