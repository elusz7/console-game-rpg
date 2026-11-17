
using ConsoleGame.Models;
using ConsoleGame.Factories.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Models.FloorTests;

[TestClass]
public class AssignMonstersToRoomsTests
{
    private Mock<IItemFactory> _itemFactoryMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemFactoryMock = new Mock<IItemFactory>();
        _randomProviderMock = new Mock<IRandomProvider>();

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => min);
    }

    private Floor CreateFloor(
        List<Room>? rooms = null,
        List<Monster>? monsters = null)
    {
        return new Floor(
            _itemFactoryMock.Object,
            _randomProviderMock.Object)
        {
            Level = 1,
            Rooms = rooms ?? [],
            Monsters = monsters ?? []
        };
    }

    private static Room CreateRoom(string name)
    {
        return new Room { Name = name };
    }

    private static Monster CreateNormalMonster()
    {
        return new Monster();
    }

    private static EliteMonster CreateEliteMonster()
    {
        return new EliteMonster();
    }

    private static BossMonster CreateBossMonster()
    {
        return new BossMonster();
    }

    // Room validation

    [TestMethod]
    public void AssignMonstersToRooms_NoRooms_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor();

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    [TestMethod]
    public void AssignMonstersToRooms_OnlyEntrance_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance")]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    [TestMethod]
    public void AssignMonstersToRooms_NoEntrance_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms: [CreateRoom("Hall"), CreateRoom("Chamber")]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    [TestMethod]
    public void AssignMonstersToRooms_ValidRoomsWithoutMonsters_DoesNotThrow()
    {
        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")]);

        floor.AssignMonstersToRooms();

        Assert.AreEqual(0, floor.Rooms.Sum(r => r.Monsters.Count));
    }

    // Boss placement

    [TestMethod]
    public void AssignMonstersToRooms_OneBoss_AssignsBossToNonEntranceRoom()
    {
        var boss = CreateBossMonster();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Boss Room")],
            monsters: [boss]);

        floor.AssignMonstersToRooms();

        Assert.IsNotNull(boss.Room);
        Assert.AreEqual("Boss Room", boss.Room.Name);
    }

    [TestMethod]
    public void AssignMonstersToRooms_MultipleBosses_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters:
            [
                CreateBossMonster(),
                CreateBossMonster()
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    [TestMethod]
    public void AssignMonstersToRooms_BossAndElite_AssignsDifferentRooms()
    {
        var boss = CreateBossMonster();
        var elite = CreateEliteMonster();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters: [boss, elite]);

        floor.AssignMonstersToRooms();

        Assert.IsNotNull(boss.Room);
        Assert.IsNotNull(elite.Room);
        Assert.AreNotSame(boss.Room, elite.Room);
    }

    [TestMethod]
    public void AssignMonstersToRooms_BossWithoutAvailableRoom_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance")],
            monsters: [CreateBossMonster()]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    // Elite placement

    [TestMethod]
    public void AssignMonstersToRooms_OneElite_AssignsEliteToNonEntranceRoom()
    {
        var elite = CreateEliteMonster();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")],
            monsters: [elite]);

        floor.AssignMonstersToRooms();

        Assert.IsNotNull(elite.Room);
        Assert.AreEqual("Hall", elite.Room.Name);
    }

    [TestMethod]
    public void AssignMonstersToRooms_MultipleElites_AssignsDifferentRooms()
    {
        var elites = new List<Monster>
        {
            CreateEliteMonster(),
            CreateEliteMonster(),
            CreateEliteMonster()
        };

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2"),
                CreateRoom("Room 3")
            ],
            monsters: elites);

        floor.AssignMonstersToRooms();

        Assert.AreEqual(
            3,
            elites.Select(m => m.Room).Distinct().Count());
    }

    [TestMethod]
    public void AssignMonstersToRooms_TooManyElites_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1")
            ],
            monsters:
            [
                CreateEliteMonster(),
                CreateEliteMonster()
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    [TestMethod]
    public void AssignMonstersToRooms_EliteAndNormalMonsters_AssignsDifferentRooms()
    {
        var elite = CreateEliteMonster();
        var normal = CreateNormalMonster();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Elite Room"),
                CreateRoom("Normal Room")
            ],
            monsters: [elite, normal]);

        floor.AssignMonstersToRooms();

        Assert.AreNotSame(elite.Room, normal.Room);
    }

    [TestMethod]
    public void AssignMonstersToRooms_ElitesOccupyAllRoomsWithNormalMonsters_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters:
            [
                CreateEliteMonster(),
                CreateEliteMonster(),
                CreateNormalMonster()
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.AssignMonstersToRooms);
    }

    // Normal monster distribution

    [TestMethod]
    public void AssignMonstersToRooms_OneNormalMonster_AssignsToNonEntranceRoom()
    {
        var monster = CreateNormalMonster();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")],
            monsters: [monster]);

        floor.AssignMonstersToRooms();

        Assert.IsNotNull(monster.Room);
        Assert.AreEqual("Hall", monster.Room.Name);
    }

    [TestMethod]
    public void AssignMonstersToRooms_MultipleNormalMonsters_AssignsAllMonsters()
    {
        var monsters = new List<Monster>
        {
            CreateNormalMonster(),
            CreateNormalMonster(),
            CreateNormalMonster()
        };

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        Assert.IsTrue(monsters.All(m => m.Room != null));
    }

    [TestMethod]
    public void AssignMonstersToRooms_FewerMonstersThanRooms_UsesDistinctRooms()
    {
        var monsters = new List<Monster>
        {
            CreateNormalMonster(),
            CreateNormalMonster()
        };

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2"),
                CreateRoom("Room 3")
            ],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        Assert.AreNotSame(monsters[0].Room, monsters[1].Room);
    }

    [TestMethod]
    public void AssignMonstersToRooms_MoreMonstersThanRooms_ReusesRooms()
    {
        var monsters = Enumerable.Range(0, 5)
            .Select(_ => CreateNormalMonster())
            .ToList();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        Assert.AreEqual(5, monsters.Count(m => m.Room != null));
        Assert.IsTrue(floor.Rooms.Any(r => r.Monsters.Count > 1));
    }

    [TestMethod]
    public void AssignMonstersToRooms_RoomsExhausted_ResetsAvailableRooms()
    {
        var monsters = Enumerable.Range(0, 4)
            .Select(_ => CreateNormalMonster())
            .ToList();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        var room1 = floor.Rooms.Single(r => r.Name == "Room 1");
        var room2 = floor.Rooms.Single(r => r.Name == "Room 2");

        Assert.AreEqual(2, room1.Monsters.Count);
        Assert.AreEqual(2, room2.Monsters.Count);
    }

    [TestMethod]
    public void AssignMonstersToRooms_NormalMonstersNeverAssignedToEntrance()
    {
        var monsters = Enumerable.Range(0, 5)
            .Select(_ => CreateNormalMonster())
            .ToList();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        var entrance = floor.GetEntrance();

        Assert.AreEqual(0, entrance.Monsters.Count);
        Assert.IsTrue(monsters.All(m => m.Room != entrance));
    }

    [TestMethod]
    public void AssignMonstersToRooms_NormalMonstersNeverAssignedToBossRoom()
    {
        var boss = CreateBossMonster();
        var normal = CreateNormalMonster();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Boss Room"),
                CreateRoom("Normal Room")
            ],
            monsters: [boss, normal]);

        floor.AssignMonstersToRooms();

        Assert.AreNotSame(boss.Room, normal.Room);
        Assert.AreEqual(1, boss.Room!.Monsters.Count);
    }

    [TestMethod]
    public void AssignMonstersToRooms_NormalMonstersNeverAssignedToEliteRoom()
    {
        var elite = CreateEliteMonster();
        var normal = CreateNormalMonster();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Elite Room"),
                CreateRoom("Normal Room")
            ],
            monsters: [elite, normal]);

        floor.AssignMonstersToRooms();

        Assert.AreNotSame(elite.Room, normal.Room);
        Assert.AreEqual(1, elite.Room!.Monsters.Count);
    }

    // Assignment consistency

    [TestMethod]
    public void AssignMonstersToRooms_AssignedMonster_HasCorrectRoomReference()
    {
        var room = CreateRoom("Hall");
        var monster = CreateNormalMonster();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), room],
            monsters: [monster]);

        floor.AssignMonstersToRooms();

        Assert.AreSame(room, monster.Room);
    }

    [TestMethod]
    public void AssignMonstersToRooms_AssignedMonster_AppearsInRoomMonsterList()
    {
        var room = CreateRoom("Hall");
        var monster = CreateNormalMonster();

        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), room],
            monsters: [monster]);

        floor.AssignMonstersToRooms();

        CollectionAssert.Contains(room.Monsters, monster);
    }

    [TestMethod]
    public void AssignMonstersToRooms_EachMonsterAssignedExactlyOnce()
    {
        var monsters = Enumerable.Range(0, 5)
            .Select(_ => CreateNormalMonster())
            .ToList();

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Entrance"),
                CreateRoom("Room 1"),
                CreateRoom("Room 2")
            ],
            monsters: monsters);

        floor.AssignMonstersToRooms();

        foreach (var monster in monsters)
        {
            Assert.AreEqual(
                1,
                floor.Rooms.Sum(r => r.Monsters.Count(m => ReferenceEquals(m, monster))));
        }
    }

    [TestMethod]
    public void AssignMonstersToRooms_UsesInjectedRandomProvider()
    {
        var floor = CreateFloor(
            rooms: [CreateRoom("Entrance"), CreateRoom("Hall")],
            monsters: [CreateNormalMonster()]);

        floor.AssignMonstersToRooms();

        _randomProviderMock.Verify(
            r => r.Next(0, 1),
            Times.Once);
    }
}
