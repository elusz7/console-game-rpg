
using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Models.FloorTests;

[TestClass]
public class AssignItemsToMonstersTests
{
    private Mock<IItemFactory> _itemFactoryMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemFactoryMock = new Mock<IItemFactory>();
        _randomProviderMock = new Mock<IRandomProvider>();

        _randomProviderMock
            .Setup(r => r.Next())
            .Returns(0);
    }

    private Floor CreateFloor(
        List<Monster>? monsters = null,
        List<Item>? loot = null)
    {
        return new Floor(
            _itemFactoryMock.Object,
            _randomProviderMock.Object)
        {
            Level = 1,
            Monsters = monsters ?? [],
            Loot = loot ?? []
        };
    }

    private static Monster CreateMonster()
    {
        return new Monster();
    }

    private Item CreateItem(string name)
    {
        return new Valuable
        {
            Name = name,
            Description = "Test item",
            Weight = 1,
            RequiredLevel = 1
        };
    }

    // Basic item assignment

    [TestMethod]
    public void AssignItemsToMonsters_NoLoot_DoesNotAssignItems()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster()
        };

        var floor = CreateFloor(monsters: monsters);

        floor.AssignItemsToMonsters();

        Assert.IsTrue(monsters.All(m => m.Treasure == null));
    }

    [TestMethod]
    public void AssignItemsToMonsters_OneItem_AssignsToOneMonster()
    {
        var monster = CreateMonster();
        var item = CreateItem("Gold Ring");

        var floor = CreateFloor(
            monsters: [monster],
            loot: [item]);

        floor.AssignItemsToMonsters();

        Assert.AreSame(item, monster.Treasure);
    }

    [TestMethod]
    public void AssignItemsToMonsters_MultipleItems_AssignsAllItems()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster(),
            CreateMonster()
        };

        var loot = new List<Item>
        {
            CreateItem("Gold Ring"),
            CreateItem("Silver Necklace"),
            CreateItem("Ruby")
        };

        var floor = CreateFloor(monsters, loot);

        floor.AssignItemsToMonsters();

        foreach (var item in loot)
        {
            Assert.IsTrue(monsters.Any(m => ReferenceEquals(m.Treasure, item)));
        }
    }

    [TestMethod]
    public void AssignItemsToMonsters_FewerItemsThanMonsters_LeavesSomeWithoutTreasure()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster(),
            CreateMonster()
        };

        var floor = CreateFloor(
            monsters: monsters,
            loot: [CreateItem("Gold Ring")]);

        floor.AssignItemsToMonsters();

        Assert.AreEqual(1, monsters.Count(m => m.Treasure != null));
        Assert.AreEqual(2, monsters.Count(m => m.Treasure == null));
    }

    [TestMethod]
    public void AssignItemsToMonsters_EqualItemsAndMonsters_AssignsOneItemPerMonster()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster(),
            CreateMonster()
        };

        var loot = new List<Item>
        {
            CreateItem("Gold Ring"),
            CreateItem("Silver Necklace"),
            CreateItem("Ruby")
        };

        var floor = CreateFloor(monsters, loot);

        floor.AssignItemsToMonsters();

        Assert.IsTrue(monsters.All(m => m.Treasure != null));
        Assert.AreEqual(3, monsters.Select(m => m.Treasure).Distinct().Count());
    }

    [TestMethod]
    public void AssignItemsToMonsters_MoreItemsThanMonsters_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            monsters: [CreateMonster()],
            loot:
            [
                CreateItem("Gold Ring"),
                CreateItem("Silver Necklace")
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignItemsToMonsters);
    }

    // Existing treasure

    [TestMethod]
    public void AssignItemsToMonsters_MonsterWithExistingTreasure_IsSkipped()
    {
        var existingTreasure = CreateItem("Existing Treasure");
        var newTreasure = CreateItem("New Treasure");

        var occupiedMonster = CreateMonster();
        occupiedMonster.Treasure = existingTreasure;

        var availableMonster = CreateMonster();

        var floor = CreateFloor(
            monsters: [occupiedMonster, availableMonster],
            loot: [newTreasure]);

        floor.AssignItemsToMonsters();

        Assert.AreSame(existingTreasure, occupiedMonster.Treasure);
        Assert.AreSame(newTreasure, availableMonster.Treasure);
    }

    [TestMethod]
    public void AssignItemsToMonsters_ExistingTreasure_IsNotOverwritten()
    {
        var existingTreasure = CreateItem("Existing Treasure");

        var monster = CreateMonster();
        monster.Treasure = existingTreasure;

        var floor = CreateFloor(
            monsters: [monster]);

        floor.AssignItemsToMonsters();

        Assert.AreSame(existingTreasure, monster.Treasure);
    }

    [TestMethod]
    public void AssignItemsToMonsters_InsufficientEligibleMonsters_ThrowsInvalidOperationException()
    {
        var occupiedMonster = CreateMonster();
        occupiedMonster.Treasure = CreateItem("Existing Treasure");

        var availableMonster = CreateMonster();

        var floor = CreateFloor(
            monsters: [occupiedMonster, availableMonster],
            loot:
            [
                CreateItem("Gold Ring"),
                CreateItem("Silver Necklace")
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignItemsToMonsters);
    }

    [TestMethod]
    public void AssignItemsToMonsters_AllMonstersHaveTreasureAndNoLoot_DoesNotThrow()
    {
        var monster1 = CreateMonster();
        var monster2 = CreateMonster();

        var treasure1 = CreateItem("Gold Ring");
        var treasure2 = CreateItem("Silver Necklace");

        monster1.Treasure = treasure1;
        monster2.Treasure = treasure2;

        var floor = CreateFloor(
            monsters: [monster1, monster2]);

        floor.AssignItemsToMonsters();

        Assert.AreSame(treasure1, monster1.Treasure);
        Assert.AreSame(treasure2, monster2.Treasure);
    }

    // Assignment consistency

    [TestMethod]
    public void AssignItemsToMonsters_EachItemAssignedExactlyOnce()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster(),
            CreateMonster()
        };

        var loot = new List<Item>
        {
            CreateItem("Gold Ring"),
            CreateItem("Silver Necklace")
        };

        var floor = CreateFloor(monsters, loot);

        floor.AssignItemsToMonsters();

        foreach (var item in loot)
        {
            Assert.AreEqual(
                1,
                monsters.Count(m => ReferenceEquals(m.Treasure, item)));
        }
    }

    [TestMethod]
    public void AssignItemsToMonsters_AssignedTreasure_ReferencesOriginalLootItem()
    {
        var monster = CreateMonster();
        var item = CreateItem("Gold Ring");

        var floor = CreateFloor(
            monsters: [monster],
            loot: [item]);

        floor.AssignItemsToMonsters();

        Assert.AreSame(item, monster.Treasure);
        Assert.AreSame(item, floor.Loot[0]);
    }

    [TestMethod]
    public void AssignItemsToMonsters_UsesInjectedRandomProvider()
    {
        var monsters = new List<Monster>
        {
            CreateMonster(),
            CreateMonster()
        };

        var loot = new List<Item>
        {
            CreateItem("Gold Ring"),
            CreateItem("Silver Necklace")
        };

        var floor = CreateFloor(monsters, loot);

        floor.AssignItemsToMonsters();

        // Two calls for monster ordering,
        // two calls for item ordering.
        _randomProviderMock.Verify(
            r => r.Next(),
            Times.Exactly(4));
    }

    [TestMethod]
    public void AssignItemsToMonsters_NoMonstersAndNoLoot_DoesNotThrow()
    {
        var floor = CreateFloor();

        floor.AssignItemsToMonsters();

        Assert.AreEqual(0, floor.Monsters.Count);
        Assert.AreEqual(0, floor.Loot.Count);
    }
}