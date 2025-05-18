using ConsoleGameEntities.Main.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConsoleGameEntities.Main.Data
{
    public class GameContextFactory : IDesignTimeDbContextFactory<GameContext>
    {
        public GameContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = ConfigurationHelper.GetConfiguration();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            //var connectionString = configuration.GetConnectionString("ProductionConnection");

            // Build options
            var optionsBuilder = new DbContextOptionsBuilder<GameContext>();
            ConfigurationHelper.ConfigureDbContextOptions(optionsBuilder, connectionString);

            // Create and return the context
            return new GameContext(optionsBuilder.Options);
        }
    }
}
