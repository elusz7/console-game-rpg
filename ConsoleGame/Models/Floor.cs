using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Models;

public class Floor(IItemFactory itemFactory, IRandomProvider randomProvider)
{
    private readonly IItemFactory _itemFactory = itemFactory;
    private readonly IRandomProvider _randomProvider = randomProvider;

    public int Level { get; init; }
    public List<Monster> Monsters { get; init; } = [];
    public List<Room> Rooms { get; init; } = [];
    public List<Item> Loot { get; init; } = [];
    public bool HasMetMerchant { get; set; }

    private List<Item> MerchantItems { get; set; } = [];

    public void AssignMonstersToRooms()
    {
        var allAvailableRooms = Rooms.Where(r => r.Name != "Entrance").ToList();

        var bossMonsters = Monsters.Where(m => m is BossMonster).ToList();
        var eliteMonsters = Monsters.Where(m => m is EliteMonster).ToList();
        var monsters = Monsters.Where(m => m is not BossMonster && m is not EliteMonster).ToList();
        
        ValidateMonsterRoomAssignment(
            bossMonsters.Count, eliteMonsters.Count, monsters.Count,
            allAvailableRooms.Count,
            Rooms.Any(r => r.Name == "Entrance"));
        
        if (bossMonsters is [BossMonster boss])
        {
            
            var randomIndex = _randomProvider.Next(0, allAvailableRooms.Count);
            var bossRoom = allAvailableRooms[randomIndex];
            
            boss.Room = bossRoom;
            bossRoom.Monsters.Add(boss);
            allAvailableRooms.Remove(bossRoom);
        }
        
        foreach (var eliteMonster in eliteMonsters)
        {
            var randomIndex = _randomProvider.Next(0, allAvailableRooms.Count);
            var eliteRoom = allAvailableRooms[randomIndex];
            
            eliteMonster.Room = eliteRoom;
            eliteRoom.Monsters.Add(eliteMonster);
            allAvailableRooms.Remove(eliteRoom);
        }
        
        var usedRooms = new List<Room>();
        foreach (var monster in monsters)
        {
            var randomRoomIndex = _randomProvider.Next(0, allAvailableRooms.Count);
            var room = allAvailableRooms[randomRoomIndex];

            monster.Room = room;
            room.Monsters.Add(monster);

            usedRooms.Add(room);
            allAvailableRooms.Remove(room);

            if (allAvailableRooms.Count == 0)
            {
                allAvailableRooms = [.. usedRooms];
                usedRooms.Clear();
            }
        }
    }

    private void ValidateMonsterRoomAssignment(int bossCount, int eliteCount, int monsterCount, int roomCount, bool hasEntrance)
    {
        if (!hasEntrance)
            throw new InvalidOperationException("Floor must have an Entrance room");
        
        if (roomCount < 1)
            throw new InvalidOperationException("At least one valid room is required for monster assignment");
        
        if (bossCount > 1)
            throw new InvalidOperationException("Only 1 boss monster allowed per floor");

        var remainingRoomCount = roomCount - bossCount - eliteCount;

        if (remainingRoomCount < 0)
            throw new InvalidOperationException("Not enough rooms for Boss and Elite monster assignment");

        if (monsterCount > 0 && remainingRoomCount == 0)
            throw new InvalidOperationException("No rooms available for non-Elite/Boss monster assignment");
    }

    public void AssignItemsToMonsters()
    {
        var availableMonsters = Monsters.Where(m => m.Treasure == null).OrderBy(_ => _randomProvider.Next()).ToList();

        if (Loot.Count > availableMonsters.Count)
        {
            throw new InvalidOperationException(
                $"Not enough monsters to assign items. Items: {Loot.Count}, Available Monsters: {availableMonsters.Count}");
        }

        var shuffledItems = Loot.OrderBy(_ => _randomProvider.Next()).ToList();

        for (int i = 0; i < shuffledItems.Count; i++)
        {
            availableMonsters[i].SetLoot(shuffledItems[i]);
        }
    }

    public Room GetEntrance()
    {
        return Rooms.First(r => r.Name == "Entrance");
    }

    public string GetMerchantName()
    {
        if (HasMetMerchant)
            return "Senmothis";
        
        return "A Mysterious Stranger";
    }

    public IReadOnlyList<Item> GetMerchantItems()
    {
        return MerchantItems;
    }

    public void RemoveMerchantItem(Item item)
    {
        MerchantItems.Remove(item);
    }

    public void UpdateMerchantItems()
    {
        MerchantItems = _itemFactory.CreateMerchantItems(Level);
    }

    public List<Item> GetMerchantGifts(ArchetypeType type, int level)
    {
        return _itemFactory.GetMerchantGifts(type, level);
    }
}