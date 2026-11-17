using ConsoleGame.Factories.Interfaces;
using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using Moq;

namespace ConsoleGameTests.Unit.ConsoleGame.Models.FloorTests;

[TestClass]
public class GeneralTests
{
    private Mock<IItemFactory> _itemFactoryMock = null!;
    private Mock<IRandomProvider> _randomProviderMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _itemFactoryMock = new Mock<IItemFactory>();
        _randomProviderMock = new Mock<IRandomProvider>();
    }

    private Floor CreateFloor(
        int level = 1,
        List<Room>? rooms = null)
    {
        return new Floor(
            _itemFactoryMock.Object,
            _randomProviderMock.Object)
        {
            Level = level,
            Rooms = rooms ?? []
        };
    }

    private static Room CreateRoom(string name)
    {
        return new Room { Name = name };
    }

    // Entrance retrieval

    [TestMethod]
    public void GetEntrance_EntranceExists_ReturnsEntranceRoom()
    {
        var entrance = CreateRoom("Entrance");

        var floor = CreateFloor(
            rooms:
            [
                entrance,
                CreateRoom("Hall")
            ]);

        var result = floor.GetEntrance();

        Assert.AreSame(entrance, result);
        Assert.AreEqual("Entrance", result.Name);
    }

    [TestMethod]
    public void GetEntrance_EntranceNotFirst_ReturnsCorrectRoom()
    {
        var entrance = CreateRoom("Entrance");

        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Hall"),
                CreateRoom("Chamber"),
                entrance
            ]);

        var result = floor.GetEntrance();

        Assert.AreSame(entrance, result);
    }

    [TestMethod]
    public void GetEntrance_NoEntrance_ThrowsInvalidOperationException()
    {
        var floor = CreateFloor(
            rooms:
            [
                CreateRoom("Hall"),
                CreateRoom("Chamber")
            ]);

        Assert.ThrowsException<InvalidOperationException>(
            floor.GetEntrance);
    }

    [TestMethod]
    public void GetEntrance_ReturnsOriginalRoomReference()
    {
        var entrance = CreateRoom("Entrance");

        var floor = CreateFloor(
            rooms:
            [
                entrance,
                CreateRoom("Hall")
            ]);

        var result = floor.GetEntrance();

        Assert.AreSame(entrance, result);
    }

    // Floor initialization

    [TestMethod]
    public void Floor_NewInstance_InitializesEmptyMonsters()
    {
        var floor = CreateFloor();

        Assert.IsNotNull(floor.Monsters);
        Assert.AreEqual(0, floor.Monsters.Count);
    }

    [TestMethod]
    public void Floor_NewInstance_InitializesEmptyRooms()
    {
        var floor = CreateFloor();

        Assert.IsNotNull(floor.Rooms);
        Assert.AreEqual(0, floor.Rooms.Count);
    }

    [TestMethod]
    public void Floor_NewInstance_InitializesEmptyLoot()
    {
        var floor = CreateFloor();

        Assert.IsNotNull(floor.Loot);
        Assert.AreEqual(0, floor.Loot.Count);
    }

    [TestMethod]
    public void Floor_NewInstance_InitializesEmptyMerchantItems()
    {
        var floor = CreateFloor();

        var merchantItems = floor.GetMerchantItems();

        Assert.IsNotNull(merchantItems);
        Assert.AreEqual(0, merchantItems.Count);
    }

    [TestMethod]
    public void Floor_NewInstance_HasNotMetMerchant()
    {
        var floor = CreateFloor();

        Assert.IsFalse(floor.HasMetMerchant);
    }

    [TestMethod]
    public void Floor_LevelInitializer_StoresSpecifiedLevel()
    {
        const int expectedLevel = 5;

        var floor = CreateFloor(level: expectedLevel);

        Assert.AreEqual(expectedLevel, floor.Level);
    }
}