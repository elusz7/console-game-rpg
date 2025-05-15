using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao(GameContext context)
{
    private readonly GameContext _context = context;

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

    public Player? GetPlayerByName(string name)
    {
        return _context.Players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Player GetPlayer(Player player)
    {
        
        return _context.Players
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Include(p => p.Archetype)
            .ThenInclude(a => a.Skills)
            .Where(p => p.Id == player.Id).First();
    }

    public List<Player> GetAllPlayers(string name)
    {
        return [.. _context.Players
            .ToList()
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Player> GetAllPlayers()
    {
            return [.. _context.Players];
    }

    public List<string> GetAllPlayerArchetypes()
    {
        return [.. _context.Players
         .Select(p => p.Archetype.Name)
         .Distinct()];
    }

    public List<Player> GetAllPlayersByArchetype(string archetype)
    {
        return [.. _context.Players.ToList()
            .Where(p => p.Archetype.Name.Equals(archetype, StringComparison.OrdinalIgnoreCase))];
    }

    public List<Player> GetAllPlayersByLevel(int level)
    {
        return [.. _context.Players
            .Where(p => p.Level == level)];
    }
}
