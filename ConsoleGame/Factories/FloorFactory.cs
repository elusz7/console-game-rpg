using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;

namespace ConsoleGame.Factories;

public class FloorFactory(IMapFactory mapFactory, IMonsterFactory monsterFactory, IItemFactory itemFactory) : IFloorFactory
{
    private readonly IMapFactory _mapFactory = mapFactory;
    private readonly IMonsterFactory _monsterFactory = monsterFactory;
    private readonly IItemFactory _itemFactory = itemFactory;

    public Floor CreateFloor(int level, bool campaign, bool randomMap)
    {
        var rooms = _mapFactory.GenerateMap(level, campaign, randomMap);
        var monsters = _monsterFactory.GenerateMonsters(level, campaign);
        var (loot, numberOfCursedItems) = _itemFactory.GenerateLoot(level, monsters.Count, campaign);

        var floor = new Floor
        {
            Level = level,
            Rooms = rooms,
            Monsters = monsters,
            Loot = loot,
            NumberOfCursedItems = numberOfCursedItems,
            ItemFactory = _itemFactory,
        };

        floor.AssignCursesToItems();
        floor.AssignItemsToMonsters();
        floor.AssignMonstersToRooms();
        floor.UpdateMerchantItems();


        System.Diagnostics.Debug.WriteLine($"Total Loot Value: {floor.Loot.Sum(l => l.Value)}");
        foreach (var room in floor.Rooms)
        {
            if (room.Monsters.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine($"--{room.Name}--");
                foreach (var monster in room.Monsters)
                    System.Diagnostics.Debug.WriteLine($"\t[{monster.Name}] Loot: {monster.Treasure?.Name}");
            }
        }

        return floor;
    }
}
