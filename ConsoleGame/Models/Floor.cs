using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Models;

public class Floor
{
    private static readonly Random _rng = Random.Shared;

    public IItemFactory ItemFactory;
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
            room.Monsters.Add(monster);

            usedRooms.Add(room);
            availableRooms.Remove(room);

            if (availableRooms.Count == 0)
            {
                availableRooms = [.. usedRooms];
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
        var availableItems = Loot.OfType<ICursable>().ToList();

        if (NumberOfCursedItems > availableItems.Count)
        {
            throw new InvalidOperationException($"Not enough items to assign cursed items. Cursed Items: {NumberOfCursedItems}, Available Items: {availableItems.Count}");
        }

        var shuffledItems = availableItems.OrderBy(_ => _rng.Next()).ToList();
        for (int i = 0; i < NumberOfCursedItems; i++)
        {
            shuffledItems[i].Curse();
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
            return "A Mysterious Stranger";
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
        MerchantItems = ItemFactory.CreateMerchantItems(Level);
    }

    public List<Item> GetMerchantGifts(ArchetypeType type, int level)
    {
        return ItemFactory.GetMerchantGifts(type, level);
    }

}
