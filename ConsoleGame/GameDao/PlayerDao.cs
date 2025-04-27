using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Characters;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao
{
    private readonly GameContext _context;

    public PlayerDao(GameContext context)
    {
        _context = context;
    }

    public void AddPlayer(Player player)
    {
        _context.Players.Add(player);
        _context.SaveChanges();
    }

    public void UpdatePlayer(Player player)
    {
        _context.Players.Update(player);
        _context.SaveChanges();
    }

    public void DeletePlayer(Player player)
    {
        _context.Players.Remove(player);
        _context.SaveChanges();
    }

    public Player GetPlayerByName(string name)
    {
        return _context.Players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public List<Player> GetAllPlayers(string name)
    {
        return _context.Players
            .ToList()
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    public List<Player> GetAllPlayers()
    {
        return _context.Players.ToList();
    }
}
