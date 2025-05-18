using ConsoleGameEntities.Main.Exceptions;
using ConsoleGameEntities.Main.Models.Items;

namespace ConsoleGameEntities.Main.Models.Entities;

public interface IInventory
{
    int Id { get; set; }
    int Gold { get; set; }
    decimal Capacity { get; set; }
    Player Player { get; set; }
    ICollection<Item> Items { get; set; }

    void AddItem(Item item);
    void RemoveItem(Item item);
    
}