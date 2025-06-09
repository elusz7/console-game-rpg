using ConsoleGame.Factories;
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGame.Managers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGame.Menus;
using ConsoleGame.Services;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
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

        if (connectionString == null)
        {
            throw new ArgumentException("Connection string can't be empty!");
        }

        services.AddDbContext<GameContext>(options =>
        {
            ConfigurationHelper.ConfigureDbContextOptions(options, connectionString);
        });

        services.AddScoped<IRoomDao, RoomDao>();
        services.AddScoped<IPlayerDao, PlayerDao>();
        services.AddScoped<IItemDao, ItemDao>();
        services.AddScoped<IInventoryDao, InventoryDao>();
        services.AddScoped<IArchetypeDao, ArchetypeDao>();
        services.AddScoped<IMonsterDao, MonsterDao>();
        services.AddScoped<ISkillDao, SkillDao>();
        services.AddScoped<IRecipeDao, RecipeDao>();
        services.AddScoped<IRuneDao, RuneDao>();

        services.AddTransient<GameEngine>();
        services.AddTransient<StartMenu>();
        services.AddTransient<AdminMenu>();

        services.AddTransient<InventoryMenu>();
        services.AddTransient<InventoryManagement>();
        services.AddTransient<ItemManagement>();
        services.AddTransient<ItemDisplay>();

        services.AddTransient<PlayerMenu>();
        services.AddTransient<PlayerDisplay>();
        services.AddTransient<PlayerManagement>();

        services.AddTransient<RoomMenu>();
        services.AddTransient<RoomDisplay>();
        services.AddTransient<IMapManager, MapManager>();
        services.AddTransient<IMapHelper, MapHelper>();
        services.AddTransient<RoomManagement>();
        services.AddTransient<RoomConnectionManagement>();

        services.AddTransient<MonsterMenu>();
        services.AddTransient<MonsterDisplay>();
        services.AddTransient<MonsterManagement>();

        services.AddTransient<SkillMenu>();
        services.AddTransient<SkillDisplay>();
        services.AddTransient<SkillManagement>();

        services.AddTransient<ArchetypeMenu>();
        services.AddTransient<ArchetypeDisplay>();
        services.AddTransient<ArchetypeManagement>();

        services.AddTransient<IFloorFactory, FloorFactory>();
        services.AddTransient<IMonsterFactory, MonsterFactory>();
        services.AddTransient<IItemFactory, ItemFactory>();
        services.AddTransient<IMapFactory, MapFactory>();

        services.AddTransient<AdventureService>();
        services.AddTransient<CombatHelper>();
        services.AddTransient<MerchantHelper>();
        services.AddTransient<EquipmentHelper>();
        services.AddTransient<PlayerHelper>();
        services.AddTransient<LootHelper>();

        services.AddSingleton<IInputManager, InputManager>();
        services.AddSingleton<IOutputManager, OutputManager>();
        services.AddSingleton<IMonsterSkillSelector, MonsterSkillSelector>();
    }
}
