using ConsoleGame.Models;

namespace ConsoleGame.Factories;

public class FloorFactory(MapFactory mapFactory, MonsterFactory monsterFactory, ItemFactory itemFactory)
{
    private readonly MapFactory _mapFactory = mapFactory;
    private readonly MonsterFactory _monsterFactory = monsterFactory;
    private readonly ItemFactory _itemFactory = itemFactory;

    public Floor CreateFloor(int level, bool campaign)
    {
        var rooms = _mapFactory.GenerateMap(level, campaign);
        var monsters = _monsterFactory.GenerateMonsters(level, campaign);
        var (loot, numberOfCursedItems) = _itemFactory.GenerateLoot(level, monsters.Count, campaign);

        var floor = new Floor
        {
            Level = level,
            Rooms = rooms,
            Monsters = monsters,
            Loot = loot,
            NumberOfCursedItems = numberOfCursedItems
        };

        floor.AssignCursesToItems();
        floor.AssignItemsToMonsters();
        floor.AssignMonstersToRooms();

        return floor;
    }
}
