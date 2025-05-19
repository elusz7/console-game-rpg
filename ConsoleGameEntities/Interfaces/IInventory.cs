using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Entities;

public interface IInventory
{
    int Id { get; set; }
    int Gold { get; set; }
    decimal Capacity { get; set; }
    Player Player { get; set; }
    List<Item> Items { get; set; }

    void AddItem(Item item);
    void RemoveItem(Item item);
    
}