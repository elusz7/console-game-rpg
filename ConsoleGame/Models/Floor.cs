using ConsoleGame.Factories;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Models;

public class Floor
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());

    public ItemFactory ItemFactory;
    public int Level { get; set; }
    public List<Monster> Monsters { get; set; } = [];
    public List<Room> Rooms { get; set; } = [];
    public int NumberOfCursedItems { get; set; }
    public List<Item> Loot { get; set; } = [];
    public bool HasMetMerchant { get; set; } = false;
    private List<Item> MerchantItems { get; set; } = [];

    public void AssignMonstersToRooms()
    {
        var availableRooms = Rooms.Where(r => r.Name != "Entrance").ToList();
        var usedRooms = new List<Room>();

        foreach (var monster in Monsters)
        {
            var randomRoomIndex = _rng.Next(0, availableRooms.Count);
            var room = availableRooms[randomRoomIndex];
            monster.Room = room;

            usedRooms.Add(room);
            availableRooms.Remove(room);

            if (availableRooms.Count == 0)
            {
                availableRooms = new List<Room>(usedRooms);
                usedRooms.Clear();
            }
        }
    }
    public void AssignItemsToMonsters()
    {
        var availableMonsters = Monsters.Where(m => m.Treasure == null).OrderBy(_ => _rng.Next()).ToList();

        if (Loot.Count > availableMonsters.Count)
        {
            throw new InvalidOperationException($"Not enough monsters to assign items. Items: {Loot.Count}, Available Monsters: {availableMonsters.Count}");
        }

        var shuffledItems = Loot.OrderBy(_ => _rng.Next()).ToList();

        for (int i = 0; i < shuffledItems.Count; i++)
        {
            availableMonsters[i].Treasure = shuffledItems[i];
        }
    }
    public void AssignCursesToItems()
    {
        var availableItems = Loot
            .Where(i => i.ItemType.Equals("Weapon") || i.ItemType.Equals("Armor"))
            .Where(i => i.IsCursed == false).ToList();

        if (NumberOfCursedItems > availableItems.Count)
        {
            throw new InvalidOperationException($"Not enough items to assign cursed items. Cursed Items: {NumberOfCursedItems}, Available Items: {availableItems.Count}");
        }

        var shuffledItems = availableItems.OrderBy(_ => _rng.Next()).ToList();
        for (int i = 0; i < NumberOfCursedItems; i++)
        {
            shuffledItems[i].IsCursed = true;
        }
    }
    public Room GetEntrance()
    {
        return Rooms.First(r => r.Name.Equals("Entrance"));
    }
    public string GetMerchantName()
    {
        if (HasMetMerchant)
        {
            return "Senmothis";
        }
        else
        {
            return "Mysterious Stranger";
        }
    }
    public List<Item> GetMerchantItems()
    {
        return MerchantItems;
    }
    public void RemoveMerchantItem(Item item)
    {
        MerchantItems.Remove(item);
    }
    public void UpdateMerchantItems()
    {
        if (MerchantItems.Count == 0)
        {
            MerchantItems = ItemFactory.CreateMerchantItems(Level);
        }        
    }
}
