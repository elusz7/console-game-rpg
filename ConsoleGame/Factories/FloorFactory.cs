using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGame.Factories;

public class FloorFactory(IMapFactory mapFactory, IMonsterFactory monsterFactory, IItemFactory itemFactory, IRandomProvider randomProvider)
    : IFloorFactory
{
    private readonly IMapFactory _mapFactory = mapFactory;
    private readonly IMonsterFactory _monsterFactory = monsterFactory;
    private readonly IItemFactory _itemFactory = itemFactory;
    private readonly IRandomProvider _randomProvider = randomProvider;

    public Floor CreateFloor(int level, bool campaign, bool randomMap)
    {
        var rooms = _mapFactory.GenerateMap(level, campaign, randomMap);
        var monsters = _monsterFactory.GenerateMonsters(level, campaign, rooms.Count);
        var loot = _itemFactory.GenerateLoot(level, monsters.Count, campaign);

        var floor = new Floor(_itemFactory, _randomProvider)
        {
            Level = level,
            Rooms = rooms,
            Monsters = monsters,
            Loot = loot
        };

        floor.AssignItemsToMonsters();
        floor.AssignMonstersToRooms();
        floor.UpdateMerchantItems();

        //FloorDiagnosticsLoop();

        return floor;
    }
    
    public void FloorDiagnosticsLoop()
    {
        for (int i = 1; i < 21; i++)
        {
            var rooms = _mapFactory.GenerateMap(i, false, true);
            var monsters = _monsterFactory.GenerateMonsters(i, false, rooms.Count);
            var loot = _itemFactory.GenerateLoot(i, monsters.Count, false);

            var floor = new Floor(_itemFactory, _randomProvider)
            {
                Level = i,
                Rooms = rooms,
                Monsters = monsters,
                Loot = loot
            };

            floor.AssignItemsToMonsters();
            floor.AssignMonstersToRooms();
            floor.UpdateMerchantItems();

            FloorDiagnostics(floor);
        }
        
    }

    public static void FloorDiagnostics(Floor floor)
    {
        System.Diagnostics.Debug.WriteLine($"----------FLOOR LEVEL{floor.Level}-----------");
        foreach (var room in floor.Rooms)
        {
            if (room.Monsters.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine($"--{room.Name}--");
                foreach (var monster in room.Monsters)
                    System.Diagnostics.Debug.WriteLine($"\t[{monster.Name}] Loot: {monster.Treasure?.Name}");
            }
        }
        System.Diagnostics.Debug.WriteLine("\n");
    }
}