using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Characters;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao
{
    private readonly IDbContextFactory<GameContext> _contextFactory;

    public PlayerDao(IDbContextFactory<GameContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void AddPlayer(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Players.Add(player);
        context.SaveChanges();
    }

    public void UpdatePlayer(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Players.Update(player);
        context.SaveChanges();
    }

    public void DeletePlayer(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Players.Remove(player);
        context.SaveChanges();
    }

    public Player GetPlayerByName(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    }

    public List<Player> GetAllPlayers(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }
    public List<Player> GetAllPlayers()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.ToList();
    }
}
