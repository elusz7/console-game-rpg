using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.GameDao;

public class InventoryDao(GameContext context) : IInventoryDao
{
    private readonly GameContext _context = context;

    public List<Player> GetAllPlayers()
    {
        return _context.Players?.ToList() ?? [];
    }

    public void UpdateInventory(Inventory inventory)
    {
        _context.Inventories?.Update(inventory);
        _context.SaveChanges();
    }

    public List<Item> GetEquippableItems(Player player)
    {
        decimal availableCapacity = player.Inventory.Capacity - player.Inventory.GetCarryingWeight();

        return _context.Items?
            .Where(i => i.Weight <= availableCapacity)
            .Where(i => i.InventoryId == null)
            .ToList() ?? [];
    }
}
