using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class RoomDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private RoomDao _roomDao = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create and open an isolated in-memory SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Configure the real GameContext to use SQLite.
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GameContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        // Use the real DAO
        _roomDao = new RoomDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddRoom_PersistsRoom()
    {
        // Arrange
        var room = CreateRoom();

        // Act
        _roomDao.AddRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Name == room.Name);

        // Assert
        Assert.AreNotEqual(0, persistedRoom.Id);
        Assert.AreEqual(room.Name, persistedRoom.Name);
        Assert.AreEqual(room.Description, persistedRoom.Description);
    }

    [TestMethod]
    public void AddRoom_PersistsRoomProperties()
    {
        // Arrange
        var room = CreateRoom("Core Room");
        room.IsCore = true;

        // Act
        _roomDao.AddRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Name == room.Name);

        // Assert
        Assert.AreNotEqual(0, persistedRoom.Id);
        Assert.AreEqual(room.Name, persistedRoom.Name);
        Assert.AreEqual(room.Description, persistedRoom.Description);
        Assert.IsTrue(persistedRoom.IsCore);
    }

    [TestMethod]
    public void AddRoom_PersistsRoomConnections()
    {
        // Arrange
        var northRoom = CreateRoom("North Room");
        var southRoom = CreateRoom("South Room");
        var eastRoom = CreateRoom("East Room");
        var westRoom = CreateRoom("West Room");

        var room = CreateRoom("Center Room");
        room.North = northRoom;
        room.South = southRoom;
        room.East = eastRoom;
        room.West = westRoom;

        // Act
        _roomDao.AddRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Name == room.Name);

        // Assert
        Assert.AreEqual(northRoom.Id, persistedRoom.NorthId);
        Assert.AreEqual(southRoom.Id, persistedRoom.SouthId);
        Assert.AreEqual(eastRoom.Id, persistedRoom.EastId);
        Assert.AreEqual(westRoom.Id, persistedRoom.WestId);
    }

    [TestMethod]
    public void UpdateRoom_UpdatesRoomProperties()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        room.Name = "Updated Room";
        room.Description = "An updated room description.";
        room.IsCore = true;

        // Act
        _roomDao.UpdateRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Id == room.Id);

        // Assert
        Assert.AreEqual("Updated Room", persistedRoom.Name);
        Assert.AreEqual("An updated room description.", persistedRoom.Description);
        Assert.IsTrue(persistedRoom.IsCore);
    }

    [TestMethod]
    public void UpdateRoom_UpdatesRoomConnections()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        var northRoom = CreateRoom("North Room");
        var southRoom = CreateRoom("South Room");
        var eastRoom = CreateRoom("East Room");
        var westRoom = CreateRoom("West Room");

        _roomDao.AddRoom(northRoom);
        _roomDao.AddRoom(southRoom);
        _roomDao.AddRoom(eastRoom);
        _roomDao.AddRoom(westRoom);

        room.North = northRoom;
        room.South = southRoom;
        room.East = eastRoom;
        room.West = westRoom;

        // Act
        _roomDao.UpdateRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Id == room.Id);

        // Assert
        Assert.AreEqual(northRoom.Id, persistedRoom.NorthId);
        Assert.AreEqual(southRoom.Id, persistedRoom.SouthId);
        Assert.AreEqual(eastRoom.Id, persistedRoom.EastId);
        Assert.AreEqual(westRoom.Id, persistedRoom.WestId);
    }

    [TestMethod]
    public void UpdateRoom_ClearsRoomConnections()
    {
        // Arrange
        var northRoom = CreateRoom("North Room");
        var southRoom = CreateRoom("South Room");

        var room = CreateRoom();
        room.North = northRoom;
        room.South = southRoom;

        _roomDao.AddRoom(room);

        room.North = null;
        room.South = null;

        // Act
        _roomDao.UpdateRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .Single(r => r.Id == room.Id);

        // Assert
        Assert.IsNull(persistedRoom.NorthId);
        Assert.IsNull(persistedRoom.SouthId);
    }

    [TestMethod]
    public void UpdateRoom_DoesNotCreateDuplicateRoom()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        var originalId = room.Id;

        room.Description = "Updated description.";

        // Act
        _roomDao.UpdateRoom(room);

        _context.ChangeTracker.Clear();
        var rooms = _context.Rooms.ToList();

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(originalId, rooms[0].Id);
        Assert.AreEqual("Updated description.", rooms[0].Description);
    }

    [TestMethod]
    public void UpdateAllRooms_UpdatesAllRoomProperties()
    {
        // Arrange
        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");
        var room3 = CreateRoom("Room Three");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(room3);

        room1.Name = "Updated Room One";
        room1.Description = "Updated description one.";
        room1.IsCore = true;

        room2.Name = "Updated Room Two";
        room2.Description = "Updated description two.";

        room3.Name = "Updated Room Three";
        room3.Description = "Updated description three.";
        room3.IsCore = true;

        // Act
        _roomDao.UpdateAllRooms([room1, room2, room3]);

        _context.ChangeTracker.Clear();
        var rooms = _context.Rooms.OrderBy(r => r.Id).ToList();

        // Assert
        Assert.AreEqual(3, rooms.Count);

        Assert.AreEqual("Updated Room One", rooms[0].Name);
        Assert.AreEqual("Updated description one.", rooms[0].Description);
        Assert.IsTrue(rooms[0].IsCore);

        Assert.AreEqual("Updated Room Two", rooms[1].Name);
        Assert.AreEqual("Updated description two.", rooms[1].Description);
        Assert.IsFalse(rooms[1].IsCore);

        Assert.AreEqual("Updated Room Three", rooms[2].Name);
        Assert.AreEqual("Updated description three.", rooms[2].Description);
        Assert.IsTrue(rooms[2].IsCore);
    }

    [TestMethod]
    public void UpdateAllRooms_UpdatesAllRoomConnections()
    {
        // Arrange
        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");

        var northRoom = CreateRoom("North Room");
        var southRoom = CreateRoom("South Room");
        var eastRoom = CreateRoom("East Room");
        var westRoom = CreateRoom("West Room");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(northRoom);
        _roomDao.AddRoom(southRoom);
        _roomDao.AddRoom(eastRoom);
        _roomDao.AddRoom(westRoom);

        room1.North = northRoom;
        room1.East = eastRoom;

        room2.South = southRoom;
        room2.West = westRoom;

        // Act
        _roomDao.UpdateAllRooms([room1, room2]);

        _context.ChangeTracker.Clear();
        var persistedRoom1 = _context.Rooms.Single(r => r.Id == room1.Id);
        var persistedRoom2 = _context.Rooms.Single(r => r.Id == room2.Id);

        // Assert
        Assert.AreEqual(northRoom.Id, persistedRoom1.NorthId);
        Assert.AreEqual(eastRoom.Id, persistedRoom1.EastId);
        Assert.IsNull(persistedRoom1.SouthId);
        Assert.IsNull(persistedRoom1.WestId);

        Assert.AreEqual(southRoom.Id, persistedRoom2.SouthId);
        Assert.AreEqual(westRoom.Id, persistedRoom2.WestId);
        Assert.IsNull(persistedRoom2.NorthId);
        Assert.IsNull(persistedRoom2.EastId);
    }

    [TestMethod]
    public void UpdateAllRooms_HandlesEmptyList()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        // Act
        _roomDao.UpdateAllRooms([]);

        _context.ChangeTracker.Clear();
        var rooms = _context.Rooms.ToList();

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(room.Id, rooms[0].Id);
    }

    [TestMethod]
    public void UpdateAllRooms_DoesNotCreateDuplicateRooms()
    {
        // Arrange
        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);

        room1.Description = "Updated description one.";
        room2.Description = "Updated description two.";

        // Act
        _roomDao.UpdateAllRooms([room1, room2]);

        _context.ChangeTracker.Clear();
        var rooms = _context.Rooms.OrderBy(r => r.Id).ToList();

        // Assert
        Assert.AreEqual(2, rooms.Count);
        Assert.AreEqual(room1.Id, rooms[0].Id);
        Assert.AreEqual(room2.Id, rooms[1].Id);
        Assert.AreEqual("Updated description one.", rooms[0].Description);
        Assert.AreEqual("Updated description two.", rooms[1].Description);
    }

    [TestMethod]
    public void DeleteRoom_RemovesRoom()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        // Act
        _roomDao.DeleteRoom(room);

        _context.ChangeTracker.Clear();
        var persistedRoom = _context.Rooms
            .SingleOrDefault(r => r.Id == room.Id);

        // Assert
        Assert.IsNull(persistedRoom);
    }

    [TestMethod]
    public void DeleteRoom_DoesNotRemoveOtherRooms()
    {
        // Arrange
        var roomToDelete = CreateRoom("Room to Delete");
        var roomToKeep = CreateRoom("Room to Keep");

        _roomDao.AddRoom(roomToDelete);
        _roomDao.AddRoom(roomToKeep);

        // Act
        _roomDao.DeleteRoom(roomToDelete);

        _context.ChangeTracker.Clear();
        var rooms = _context.Rooms.ToList();

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(roomToKeep.Id, rooms[0].Id);
        Assert.AreEqual("Room to Keep", rooms[0].Name);
    }

    [TestMethod]
    public void DeleteRoom_PreservesAssociatedMonsters()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        var monster = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        monster.Room = room;

        _context.Monsters.Add(monster);
        _context.SaveChanges();

        // Act
        _roomDao.DeleteRoom(room);

        _context.ChangeTracker.Clear();
        var persistedMonster = _context.Monsters
            .SingleOrDefault(m => m.Id == monster.Id);

        // Assert
        Assert.IsNotNull(persistedMonster);
        Assert.AreEqual(monster.Id, persistedMonster.Id);
        Assert.AreEqual("Goblin", persistedMonster.Name);
    }

    [TestMethod]
    public void DeleteRoom_ClearsMonsterRoomAssociation()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        var monster = CreateMonster("Goblin", ThreatLevel.Low, DamageType.Martial, TestMonsterType.Common);

        monster.Room = room;

        _context.Monsters.Add(monster);
        _context.SaveChanges();

        // Act
        _roomDao.DeleteRoom(room);

        _context.ChangeTracker.Clear();
        var persistedMonster = _context.Monsters
            .Include(m => m.Room)
            .Single(m => m.Id == monster.Id);

        // Assert
        Assert.IsNull(persistedMonster.RoomId);
        Assert.IsNull(persistedMonster.Room);
    }

    [TestMethod]
    public void GetAllRooms_ReturnsAllRooms()
    {
        // Arrange
        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");
        var room3 = CreateRoom("Room Three");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(room3);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRooms();

        // Assert
        Assert.AreEqual(3, rooms.Count);
        CollectionAssert.AreEquivalent(
            new[] { room1.Id, room2.Id, room3.Id },
            rooms.Select(r => r.Id).ToArray());
    }

    [TestMethod]
    public void GetAllRooms_IncludesAssociatedMonsters()
    {
        // Arrange
        var room = CreateRoom();
        _roomDao.AddRoom(room);

        var monster1 = CreateMonster(
            "Goblin",
            ThreatLevel.Low,
            DamageType.Martial,
            TestMonsterType.Common);

        var monster2 = CreateMonster(
            "Orc",
            ThreatLevel.Medium,
            DamageType.Martial,
            TestMonsterType.Common);

        monster1.Room = room;
        monster2.Room = room;

        _context.Monsters.AddRange(monster1, monster2);
        _context.SaveChanges();

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRooms();

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(2, rooms[0].Monsters.Count);

        CollectionAssert.AreEquivalent(
            new[] { monster1.Id, monster2.Id },
            rooms[0].Monsters.Select(m => m.Id).ToArray());
    }

    [TestMethod]
    public void GetAllRooms_ReturnsEmptyListWhenNoRoomsExist()
    {
        // Arrange

        // Act
        var rooms = _roomDao.GetAllRooms();

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }

    [TestMethod]
    public void FindRoomByName_ReturnsExactNameMatch()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var foundRoom = _roomDao.FindRoomByName("Ancient Temple");

        // Assert
        Assert.IsNotNull(foundRoom);
        Assert.AreEqual(room.Id, foundRoom.Id);
        Assert.AreEqual("Ancient Temple", foundRoom.Name);
    }

    [TestMethod]
    public void FindRoomByName_ReturnsPartialNameMatch()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var foundRoom = _roomDao.FindRoomByName("Temple");

        // Assert
        Assert.IsNotNull(foundRoom);
        Assert.AreEqual(room.Id, foundRoom.Id);
        Assert.AreEqual("Ancient Temple", foundRoom.Name);
    }

    [TestMethod]
    public void FindRoomByName_IsCaseInsensitive()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var foundRoom = _roomDao.FindRoomByName("ANCIENT TEMPLE");

        // Assert
        Assert.IsNotNull(foundRoom);
        Assert.AreEqual(room.Id, foundRoom.Id);
    }

    [TestMethod]
    public void FindRoomByName_ReturnsNullWhenNoMatchExists()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var foundRoom = _roomDao.FindRoomByName("Castle");

        // Assert
        Assert.IsNull(foundRoom);
    }

    [TestMethod]
    public void RoomExists_ReturnsTrueWhenRoomExists()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var exists = _roomDao.RoomExists("Ancient Temple");

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void RoomExists_IsCaseInsensitive()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var exists = _roomDao.RoomExists("ANCIENT TEMPLE");

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void RoomExists_ReturnsFalseWhenRoomDoesNotExist()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var exists = _roomDao.RoomExists("Castle");

        // Assert
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void RoomExists_RequiresExactNameMatch()
    {
        // Arrange
        var room = CreateRoom("Ancient Temple");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var exists = _roomDao.RoomExists("Ancient");

        // Assert
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void GetEntrance_ReturnsRoomWithIdOne()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;

        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var otherRoom = CreateRoom("Other Room");
        _roomDao.AddRoom(otherRoom);

        // Act
        _context.ChangeTracker.Clear();
        var foundEntrance = _roomDao.GetEntrance();

        // Assert
        Assert.AreEqual(1, foundEntrance.Id);
        Assert.AreEqual("Entrance", foundEntrance.Name);
    }

    [TestMethod]
    public void GetEntrance_ThrowsWhenEntranceDoesNotExist()
    {
        // Arrange
        var room = CreateRoom("Other Room");
        room.Id = 2;

        _context.Rooms.Add(room);
        _context.SaveChanges();

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => _roomDao.GetEntrance());
    }

    [TestMethod]
    public void GetAllEditableRooms_ExcludesEntranceRoom()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;

        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllEditableRooms();

        // Assert
        Assert.AreEqual(2, rooms.Count);
        Assert.IsFalse(rooms.Any(r => r.Id == 1));

        CollectionAssert.AreEquivalent(
            new[] { room1.Id, room2.Id },
            rooms.Select(r => r.Id).ToArray());
    }

    [TestMethod]
    public void GetAllEditableRooms_ReturnsEmptyListWhenOnlyEntranceExists()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;

        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllEditableRooms();

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }

    [TestMethod]
    public void GetAllDeletableRooms_ExcludesCoreRooms()
    {
        // Arrange
        var coreRoom = CreateRoom("Core Room");
        coreRoom.IsCore = true;

        var nonCoreRoom1 = CreateRoom("Room One");
        var nonCoreRoom2 = CreateRoom("Room Two");

        _roomDao.AddRoom(coreRoom);
        _roomDao.AddRoom(nonCoreRoom1);
        _roomDao.AddRoom(nonCoreRoom2);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllDeletableRooms();

        // Assert
        Assert.AreEqual(2, rooms.Count);

        CollectionAssert.AreEquivalent(
            new[] { nonCoreRoom1.Id, nonCoreRoom2.Id },
            rooms.Select(r => r.Id).ToArray());

        Assert.IsFalse(rooms.Any(r => r.IsCore));
    }

    [TestMethod]
    public void GetAllDeletableRooms_ReturnsEmptyListWhenAllRoomsAreCore()
    {
        // Arrange
        var room1 = CreateRoom("Core Room One");
        var room2 = CreateRoom("Core Room Two");

        room1.IsCore = true;
        room2.IsCore = true;

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllDeletableRooms();

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }

    [TestMethod]
    public void GetAllRoomsMaxConnections_ReturnsRoomsAtOrBelowMaxConnections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;
        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room1 = CreateRoom("No Connections");
        var room2 = CreateRoom("One Connection");
        var room3 = CreateRoom("Two Connections");
        var room4 = CreateRoom("Three Connections");

        room2.North = room1;

        room3.North = room1;
        room3.South = room2;

        room4.North = room1;
        room4.South = room2;
        room4.East = room3;

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(room3);
        _roomDao.AddRoom(room4);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(2);

        // Assert
        Assert.AreEqual(3, rooms.Count);

        CollectionAssert.AreEquivalent(
            new[] { room1.Id, room2.Id, room3.Id },
            rooms.Select(r => r.Id).ToArray());
    }
    
    [TestMethod]
    public void GetAllRoomsMaxConnections_ExcludesRoomsAboveMaxConnections()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;
        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var eligibleRoom = CreateRoom("Eligible Room");
        eligibleRoom.North = entrance;

        var ineligibleRoom = CreateRoom("Ineligible Room");
        ineligibleRoom.North = entrance;
        ineligibleRoom.South = eligibleRoom;
        ineligibleRoom.East = entrance;

        _roomDao.AddRoom(eligibleRoom);
        _roomDao.AddRoom(ineligibleRoom);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(2);

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(eligibleRoom.Id, rooms[0].Id);
    }
    
    [TestMethod]
    public void GetAllRoomsMaxConnections_ExcludesEntranceRoom()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;

        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room = CreateRoom("Editable Room");
        _roomDao.AddRoom(room);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(4);

        // Assert
        Assert.AreEqual(1, rooms.Count);
        Assert.AreEqual(room.Id, rooms[0].Id);
        Assert.IsFalse(rooms.Any(r => r.Id == 1));
    }
    
    [TestMethod]
    public void GetAllRoomsMaxConnections_ReturnsRoomsWithNoConnectionsWhenMaxIsZero()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;
        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room1 = CreateRoom("Room One");
        var room2 = CreateRoom("Room Two");
        var connectedRoom = CreateRoom("Connected Room");

        connectedRoom.North = room1;

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(connectedRoom);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(0);

        // Assert
        Assert.AreEqual(2, rooms.Count);

        CollectionAssert.AreEquivalent(
            new[] { room1.Id, room2.Id },
            rooms.Select(r => r.Id).ToArray());
    }
    
    [TestMethod]
    public void GetAllRoomsMaxConnections_ReturnsAllEditableRoomsWhenMaxIsFour()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;

        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room1 = CreateRoom("No Connections");
        var room2 = CreateRoom("One Connection");
        var room3 = CreateRoom("Two Connections");
        var room4 = CreateRoom("Three Connections");
        var room5 = CreateRoom("Four Connections");

        room2.North = room1;

        room3.North = room1;
        room3.South = room2;

        room4.North = room1;
        room4.South = room2;
        room4.East = room3;

        room5.North = room1;
        room5.South = room2;
        room5.East = room3;
        room5.West = room4;

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);
        _roomDao.AddRoom(room3);
        _roomDao.AddRoom(room4);
        _roomDao.AddRoom(room5);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(4);

        // Assert
        Assert.AreEqual(5, rooms.Count);

        CollectionAssert.AreEquivalent(
            new[] { room1.Id, room2.Id, room3.Id, room4.Id, room5.Id },
            rooms.Select(r => r.Id).ToArray());

        Assert.IsFalse(rooms.Any(r => r.Id == 1));
    }
    
    [TestMethod]
    public void GetAllRoomsMaxConnections_ReturnsEmptyListWhenNoRoomsMatch()
    {
        // Arrange
        var entrance = CreateRoom("Entrance");
        entrance.Id = 1;
        _context.Rooms.Add(entrance);
        _context.SaveChanges();

        var room1 = CreateRoom("Three Connections One");
        var room2 = CreateRoom("Three Connections Two");

        _roomDao.AddRoom(room1);
        _roomDao.AddRoom(room2);

        room1.North = entrance;
        room1.South = room2;
        room1.East = entrance;

        room2.North = entrance;
        room2.South = room1;
        room2.West = entrance;

        _roomDao.UpdateRoom(room1);
        _roomDao.UpdateRoom(room2);

        // Act
        _context.ChangeTracker.Clear();
        var rooms = _roomDao.GetAllRoomsMaxConnections(2);

        // Assert
        Assert.AreEqual(0, rooms.Count);
    }
}