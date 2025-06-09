using ConsoleGameEntities.Models.Items;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao.Interfaces;

public interface IItemDao
{
    string SortOrder { get; set; }

    void AddItem(Item item);
    void UpdateItem(Item item);
    void DeleteItem(Item item);
    List<Item> GetItemsByName(string name);
    List<Item> GetAllItems();
    List<Item> GetAllNonCoreItems();
    List<Item> GetItemsByType(ItemType type);
    List<Item> GetItemsByMaxLevel(int level);
}
