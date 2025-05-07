using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGame.Services;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Helpers;
using ConsoleGame.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace ConsoleGame;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Build configuration
        var configuration = ConfigurationHelper.GetConfiguration();

        // Create and bind FileLoggerOptions
        var fileLoggerOptions = new NReco.Logging.File.FileLoggerOptions();
        configuration.GetSection("Logging:File").Bind(fileLoggerOptions);

        // Configure logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));

            // Add Console logger
            loggingBuilder.AddConsole();

            // Add File logger using the correct constructor
            var logFileName = "Logs/log.txt"; // Specify the log file path

            loggingBuilder.AddProvider(new FileLoggerProvider(logFileName, fileLoggerOptions));
        });

        // Register DbContext with dependency injection
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<GameContext>(options =>
        {
            ConfigurationHelper.ConfigureDbContextOptions(options, connectionString);
        });


        services.AddScoped<RoomDao>();
        services.AddScoped<PlayerDao>();
        services.AddScoped<ItemDao>();
        services.AddScoped<InventoryDao>();
        services.AddScoped<ArchetypeDao>();
        services.AddScoped<MonsterDao>();
        services.AddScoped<SkillDao>();

        services.AddTransient<GameEngine>();
        services.AddTransient<StartMenuManager>();

        services.AddTransient<InventoryManager>();
        services.AddTransient<InventoryManagement>();
        services.AddTransient<ItemManagement>();
        services.AddTransient<ItemDisplay>();

        services.AddTransient<PlayerManager>();
        services.AddTransient<PlayerDisplay>();
        services.AddTransient<PlayerManagement>();

        services.AddTransient<RoomManager>();
        services.AddTransient<RoomDisplay>();
        services.AddTransient<MapManager>();
        services.AddTransient<RoomManagement>();
        services.AddTransient<RoomConnectionManagement>();

        services.AddTransient<MonsterManager>();
        services.AddTransient<MonsterDisplay>();
        services.AddTransient<MonsterManagement>();

        services.AddTransient<SkillManager>();
        services.AddTransient<SkillDisplay>();
        services.AddTransient<SkillManagement>();

        services.AddSingleton<InputManager>();
        services.AddSingleton<OutputManager>();
    }
}
