using ConsoleGame.Factories;
using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Factories;

[TestClass]
public class FloorFactoryTests
{
    private Mock<IMapFactory> _mockMapFactory;
    private Mock<IMonsterFactory> _mockMonsterFactory;
    private Mock<IItemFactory> _mockItemFactory;
    private FloorFactory _floorFactory;

    private List<Room> _rooms;
    private List<Monster> _monsters;
    private List<Item> _loot;
    private int _curseCount;

    [TestInitialize]
    public void Setup()
    {
        _mockMapFactory = new Mock<IMapFactory>();
        _mockMonsterFactory = new Mock<IMonsterFactory>();
        _mockItemFactory = new Mock<IItemFactory>();

        _floorFactory = new FloorFactory(
            _mockMapFactory.Object,
            _mockMonsterFactory.Object,
            _mockItemFactory.Object
        );

        _rooms = [new Room { Id = 1, Name = "Test Room" }];
        _monsters = [new Monster { Name = "Goblin", MaxHealth = 10 }];
        _loot = [new Weapon { Name = "Sword", Value = 10 }];
        _curseCount = 1;

        _mockMapFactory
            .Setup(m => m.GenerateMap(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(_rooms);

        _mockMonsterFactory
            .Setup(m => m.GenerateMonsters(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(_monsters);

        _mockItemFactory
            .Setup(m => m.GenerateLoot(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns((_loot, _curseCount));
    }

    [TestMethod]
    public void CreateFloor_CreatesFloorWithExpectedValues()
    {
        // Act
        var floor = _floorFactory.CreateFloor(2, true, false);

        // Assert
        Assert.AreEqual(2, floor.Level);
        CollectionAssert.AreEqual(_rooms, floor.Rooms);
        CollectionAssert.AreEqual(_monsters, floor.Monsters);
        CollectionAssert.AreEqual(_loot, floor.Loot);
        Assert.AreEqual(_curseCount, floor.NumberOfCursedItems);
        Assert.AreEqual(_mockItemFactory.Object, floor.ItemFactory);
    }

    [TestMethod]
    public void CreateFloor_CallsAllFactoriesWithExpectedArguments()
    {
        // Act
        var floor = _floorFactory.CreateFloor(5, false, true);

        // Assert
        _mockMapFactory.Verify(m => m.GenerateMap(5, false, true), Times.Once);
        _mockMonsterFactory.Verify(m => m.GenerateMonsters(5, false), Times.Once);
        _mockItemFactory.Verify(m => m.GenerateLoot(5, _monsters.Count, false), Times.Once);
    }
}