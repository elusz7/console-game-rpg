using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao(GameContext context) : IPlayerDao
{
    private readonly GameContext _context = context;

    public void AddPlayer(Player player)
    {
        _context.Players.Add(player);
        _context.SaveChanges();

        // The runtime player is managed separately during gameplay.
        // Detach it and its inventory so later room changes don't
        // cause EF Core to track generated room instances.
        _context.Entry(player).State = EntityState.Detached;
        _context.Entry(player.Inventory).State = EntityState.Detached;
    }

    public void UpdatePlayer(Player player)
    {
        // Save changes to the player and related entities already tracked by EF Core.
        _context.SaveChanges();
    }

    public void DeletePlayer(Player player)
    {
        _context.Players.Remove(player);
        _context.SaveChanges();
    }

    public Player? GetPlayerByName(string name)
    {
        // Load the player together with its inventory and items.
        // EF Core tracks these entities so their changes can be saved.
        var player = _context.Players
            .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items).FirstOrDefault();
        
        player?.RebuildEquipment();

        return player;
    }

    public Player? GetPlayerByIdNoTracking(int playerId)
    {
        var player = _context.Players
            .AsNoTracking()
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Include(p => p.Inventory)
                .ThenInclude(i => i.Runes)
                    .ThenInclude(ir => ir.Rune)
            .Include(p => p.Inventory)
                .ThenInclude(i => i.Ingredients)
                    .ThenInclude(ii => ii.Ingredient)
            .Include(p => p.Archetype)
                .ThenInclude(a => a.Skills)
            .FirstOrDefault(p => p.Id == playerId);
        
        player?.RebuildEquipment();

        return player;
    }

    public List<Player> GetAllPlayers(string name)
    {
        return
        [
            .. _context.Players
                .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
        ];
    }

    public List<Player> GetAllPlayers()
    {
        return [.. _context.Players];
    }

    public List<Archetype> GetDistinctPlayerArchetypes()
    {
        return
        [
            .. _context.Players
                .Select(p => p.Archetype)
                .Distinct()
        ];
    }

    public List<Player> GetPlayersByArchetypeId(int archetypeId)
    {
        return
        [
            .. _context.Players
                .Where(p => p.ArchetypeId == archetypeId)
        ];
    }

    public List<Player> GetPlayersByLevel(int level)
    {
        return
        [
            .. _context.Players
                .Where(p => p.Level == level)
        ];
    }
}