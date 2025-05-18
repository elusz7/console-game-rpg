using ConsoleGameEntities.Main.Data;
using ConsoleGameEntities.Main.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao(GameContext context)
{
    private readonly GameContext _context = context;

    public void AddPlayer(Player player)
    {
        _context.Players!.Add(player);
        _context.SaveChanges();
    }

    public void UpdatePlayer(Player player)
    {
        _context.Players!.Update(player);
        _context.SaveChanges();
    }

    public void DeletePlayer(Player player)
    {
        _context.Players!.Remove(player);
        _context.SaveChanges();
    }

    public Player? GetPlayerByName(string name)
    {
        return _context.Players!.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
    }

    public Player GetPlayerNoTracking(Player player)
    {
        return _context.Players!.AsNoTracking()
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Include(p => p.Archetype)
            .ThenInclude(a => a.Skills)
            .Include(p => p.CurrentRoom)
            .Where(p => p.Id == player.Id).First();
    }

    public List<Player> GetAllPlayers(string name)
    {
        return [.. _context.Players!
            .ToList()
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Player> GetAllPlayers()
    {
            return [.. _context.Players!];
    }
    public List<Archetype> GetAllPlayerArchetypes()
    {
        return [.. _context.Players!
         .Select(p => p.Archetype)
         .Distinct()];
    }

    public List<Player> GetAllPlayersByArchetype(Archetype archetype)
    {
        return [.. _context.Players!.ToList()
            .Where(p => p.Archetype == archetype)];
    }

    public List<Player> GetAllPlayersByLevel(int level)
    {
        return [.. _context.Players!
            .Where(p => p.Level == level)];
    }
}
