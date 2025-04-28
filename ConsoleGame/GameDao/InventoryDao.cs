using System.Linq.Expressions;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class InventoryDao
{
    private readonly GameContext _context;

    public InventoryDao(GameContext context)
    {
        _context = context;
    }

    public List<Player> GetAllPlayers()
    {
        return _context.Players.ToList();
    }
    public void UpdateInventory(Inventory inventory)
    {
        _context.Inventories.Update(inventory);
        _context.SaveChanges();
    }
    public List<Item> GetEquippableItems(Player player)
    {
        decimal availableCapacity = _context.Inventories
            .Where(i => i.PlayerId == player.Id)
            .Select(i => i.Capacity - i.Items.Sum(item => item.Weight))
            .First();

        return _context.Items
            .Where(i => i.Weight <= availableCapacity)
            .Where(i => i.InventoryId == null)
            .ToList();
    }
    public decimal GetInventoryWeight(Player player)
    {
        return _context.Inventories
            .Where(i => i.PlayerId == player.Id)
            .Select(i => i.Items.Sum(item => item.Weight))
            .First();
    }
}
