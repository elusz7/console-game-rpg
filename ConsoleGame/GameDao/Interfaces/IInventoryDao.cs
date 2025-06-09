using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.GameDao.Interfaces;

public interface IInventoryDao
{
    List<Player> GetAllPlayers();

    void UpdateInventory(Inventory inventory);

    List<Item> GetEquippableItems(Player player);
}
