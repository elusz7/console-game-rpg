using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class InventoryDao
{
    private readonly IDbContextFactory<GameContext> _contextFactory;
    public InventoryDao(IDbContextFactory<GameContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public List<Player> GetAllPlayers()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.ToList();
    }

    public void UpdateInventory(Inventory inventory)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Inventories.Update(inventory);
        context.SaveChanges();
    }

    public void AddItemToInventory(Player player, Item item)
    {
        using var context = _contextFactory.CreateDbContext();
        bool success = true;

        var inventory = context.Inventories.Include(i => i.Items).First(i => i.PlayerId == player.Id);
        
        inventory.Items.Add(item);

        context.SaveChanges();
    }
    public void RemoveItemFromInventory(Player player, Item item)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var inventory = context.Inventories.Include(i => i.Items).First(i => i.PlayerId == player.Id);

        inventory.Items.Remove(item);
        
        context.SaveChanges();
    }
    public Inventory GetInventory(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Inventories
            .Include(i => i.Items)
            .First(i => i.PlayerId == player.Id);
    }

    public List<Item> GetEquippableItems(Player player)
    {
        using var context = _contextFactory.CreateDbContext();

        decimal availableCapacity = context.Inventories
            .Where(i => i.PlayerId == player.Id)
            .Select(i => i.Capacity - i.Items.Sum(item => item.Weight))
            .First();

        return context.Items
            .Where(i => i.Weight <= availableCapacity)
            .Where(i => i.InventoryId == null)
            .ToList();
    }

    public decimal GetInventoryWeight(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Inventories
            .Where(i => i.PlayerId == player.Id)
            .Select(i => i.Items.Sum(item => item.Weight))
            .FirstOrDefault();
    }
}
