using ConsoleGame.GameDao;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ConsoleGameTests.ConsoleGame.GameDao;

[TestClass]
public class PlayerPersistenceIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private Mock<IInputManager> _inputManager = null!;
    private Mock<IOutputManager> _outputManager = null!;
    private Mock<IArchetypeDao> _archetypeDao = null!;

    private PlayerDao _playerDao = null!;
    private PlayerHelper _playerHelper = null!;

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

        // Mock dependencies unrelated to persistence.
        _inputManager = new Mock<IInputManager>();
        _outputManager = new Mock<IOutputManager>();
        _archetypeDao = new Mock<IArchetypeDao>();

        // Use the real DAO and helper.
        _playerDao = new PlayerDao(_context);

        _playerHelper = new PlayerHelper(
            _inputManager.Object,
            _outputManager.Object,
            _playerDao,
            _archetypeDao.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void SavePlayer_WhenNewPlayerIsSaved_PersistsPlayerInventoryAndItems()
    {
        // Arrange: create a valid archetype.
        var archetype = new Archetype
        {
            Name = "Warrior",
            Description = "A strong melee fighter.",
            HealthBase = 10,
            ArchetypeType = ModelEnums.ArchetypeType.Martial,
            ResourceName = "Stamina",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };

        var player = new Player
        {
            Name = "Integration Test Player",
            Archetype = archetype,
            ArchetypeId = archetype.Id
        };

        // Initialize creates the player's inventory and starting stats.
        player.Initialize();

        // Add a weapon to the player's inventory.
        var weapon = new Weapon
        {
            Name = "Iron Sword",
            Description = "A basic iron sword.",
            Value = 25M,
            Durability = 10,
            Weight = 3M,
            RequiredLevel = 1,
            AttackPower = 8,
            DamageType = ModelEnums.DamageType.Martial,
            Inventory = player.Inventory
        };

        player.Inventory.Items.Add(weapon);

        // Act: save using the real PlayerHelper and PlayerDao.
        _playerHelper.SavePlayer(player);

        // Clear tracked entities so the following query reads from SQLite.
        _context.ChangeTracker.Clear();

        // Assert: reload the complete player graph from the database.
        var savedPlayer = _context.Players!
            .Include(p => p.Archetype)
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .SingleOrDefault(p => p.Name == "Integration Test Player");

        Assert.IsNotNull(savedPlayer, "Player should be inserted.");

        Assert.IsTrue(savedPlayer.Id > 0, "Player should have a generated ID.");
        Assert.IsNotNull(savedPlayer.Archetype);
        Assert.AreEqual("Warrior", savedPlayer.Archetype.Name);

        Assert.IsNotNull(savedPlayer.Inventory);
        Assert.IsTrue(savedPlayer.Inventory.Id > 0);
        Assert.AreEqual(savedPlayer.Id, savedPlayer.Inventory.PlayerId);

        Assert.AreEqual(1, savedPlayer.Inventory.Items.Count);

        var savedWeapon = savedPlayer.Inventory.Items.Single();

        Assert.IsTrue(savedWeapon.Id > 0, "Weapon should have a generated ID.");
        Assert.AreEqual("Iron Sword", savedWeapon.Name);
        Assert.AreEqual(savedPlayer.Inventory.Id, savedWeapon.InventoryId);
    }

    [TestMethod]
    public void SavePlayer_WhenPlayerAlreadyExists_UpdatesProgressionAndPreservesInventory()
    {
        // Arrange: create and save a new player with a weapon.
        var archetype = new Archetype
        {
            Name = "Warrior",
            Description = "A strong melee fighter.",
            HealthBase = 10,
            ArchetypeType = ModelEnums.ArchetypeType.Martial,
            ResourceName = "Stamina",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };

        var player = new Player
        {
            Name = "Existing Player",
            Archetype = archetype,
            ArchetypeId = archetype.Id
        };

        player.Initialize();

        var weapon = new Weapon
        {
            Name = "Iron Sword",
            Description = "A basic iron sword.",
            Value = 25M,
            Durability = 10,
            Weight = 3M,
            RequiredLevel = 1,
            AttackPower = 8,
            DamageType = ModelEnums.DamageType.Martial,
            Inventory = player.Inventory
        };

        player.Inventory.Items.Add(weapon);

        _playerHelper.SavePlayer(player);

        // Detach the saved graph to simulate gameplay using an in-memory player.
        _context.ChangeTracker.Clear();

        // Act: change the player's current gameplay state.
        player.Level = 3;
        player.MaxHealth = 35;
        player.Inventory.Gold = 100;
        player.Inventory.Capacity = 40M;
        weapon.Durability = 6;

        _playerHelper.SavePlayer(player);

        // Clear tracking so assertions read the persisted database state.
        _context.ChangeTracker.Clear();

        var savedPlayer = _context.Players!
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Single(p => p.Name == "Existing Player");

        // Assert: progression and inventory properties were updated.
        Assert.AreEqual(3, savedPlayer.Level);
        Assert.AreEqual(35, savedPlayer.MaxHealth);
        Assert.AreEqual(100, savedPlayer.Inventory.Gold);
        Assert.AreEqual(40M, savedPlayer.Inventory.Capacity);

        // Assert: the existing weapon was preserved and updated.
        Assert.AreEqual(1, savedPlayer.Inventory.Items.Count);

        var savedWeapon = savedPlayer.Inventory.Items.Single();

        Assert.AreEqual(weapon.Id, savedWeapon.Id);
        Assert.AreEqual("Iron Sword", savedWeapon.Name);
        Assert.AreEqual(6, savedWeapon.Durability);

        // Assert: saving did not create duplicate players or inventories.
        Assert.AreEqual(1, _context.Players!.Count());
        Assert.AreEqual(1, _context.Inventories!.Count());
    }
    
    [TestMethod]
    public void SavePlayer_WhenInventoryChanges_PreservesAddsAndUnlinksRemovedItems()
    {
        // Arrange: create and save a player with two weapons.
        var archetype = new Archetype
        {
            Name = "Warrior",
            Description = "A strong melee fighter.",
            HealthBase = 10,
            ArchetypeType = ModelEnums.ArchetypeType.Martial,
            ResourceName = "Stamina",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };

        var player = new Player
        {
            Name = "Inventory Test Player",
            Archetype = archetype,
            ArchetypeId = archetype.Id
        };

        player.Initialize();

        var retainedWeapon = new Weapon
        {
            Name = "Iron Sword",
            Description = "A basic iron sword.",
            Value = 25M,
            Durability = 10,
            Weight = 3M,
            RequiredLevel = 1,
            AttackPower = 8,
            DamageType = ModelEnums.DamageType.Martial,
            Inventory = player.Inventory
        };

        var removedWeapon = new Weapon
        {
            Name = "Old Dagger",
            Description = "An old dagger.",
            Value = 10M,
            Durability = 4,
            Weight = 1M,
            RequiredLevel = 1,
            AttackPower = 3,
            DamageType = ModelEnums.DamageType.Martial,
            Inventory = player.Inventory
        };

        player.Inventory.Items.Add(retainedWeapon);
        player.Inventory.Items.Add(removedWeapon);

        _playerHelper.SavePlayer(player);

        // Simulate changes to the in-memory gameplay inventory.
        _context.ChangeTracker.Clear();

        // Remove one weapon from the player's inventory.
        player.Inventory.RemoveItem(removedWeapon);

        // Add a newly acquired weapon.
        var newWeapon = new Weapon
        {
            Name = "Steel Sword",
            Description = "A stronger sword.",
            Value = 50M,
            Durability = 15,
            Weight = 4M,
            RequiredLevel = 2,
            AttackPower = 12,
            DamageType = ModelEnums.DamageType.Martial,
            Inventory = player.Inventory
        };

        player.Inventory.AddItem(newWeapon);

        // Act: save the updated inventory.
        _playerHelper.SavePlayer(player);

        // Save again to verify repeated saves don't duplicate items.
        _playerHelper.SavePlayer(player);

        _context.ChangeTracker.Clear();

        // Reload inventory and all items from SQLite.
        var savedPlayer = _context.Players!
            .Include(p => p.Inventory)
                .ThenInclude(i => i.Items)
            .Single(p => p.Name == "Inventory Test Player");

        var allItems = _context.Items!.ToList();

        // Assert: the retained and newly acquired weapons are in inventory.
        Assert.AreEqual(2, savedPlayer.Inventory.Items.Count);

        Assert.IsTrue(savedPlayer.Inventory.Items.Any(
            item => item.Name == "Iron Sword"));

        Assert.IsTrue(savedPlayer.Inventory.Items.Any(
            item => item.Name == "Steel Sword"));

        // Assert: the removed weapon still exists but is unlinked.
        var savedRemovedWeapon = allItems.Single(
            item => item.Name == "Old Dagger");

        Assert.IsNull(savedRemovedWeapon.InventoryId);

        // Assert: retained and new weapons remain linked to the inventory.
        Assert.IsTrue(allItems.Single(
            item => item.Name == "Iron Sword").InventoryId
            == savedPlayer.Inventory.Id);

        Assert.IsTrue(allItems.Single(
            item => item.Name == "Steel Sword").InventoryId
            == savedPlayer.Inventory.Id);

        // Assert: all three item records still exist, with no duplicates.
        Assert.AreEqual(3, allItems.Count);
    }
}