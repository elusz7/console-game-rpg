using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class PlayerDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private PlayerDao _playerDao;

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

        // Use the real DAO and helper.
        _playerDao = new PlayerDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddPlayer_PersistsPlayerAndRelatedInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        // Act
        _playerDao.AddPlayer(player);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        var result = _context.Players
            .Include(p => p.Inventory)
            .Single(p => p.Name == "TestPlayer");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id > 0);

        Assert.IsTrue(result.Inventory.Id > 0);
        Assert.AreEqual(100, result.Inventory.Gold);

        Assert.AreEqual(result.Id, result.Inventory.PlayerId);
    }

    [TestMethod]
    public void AddPlayer_WithExistingArchetype_ReferencesExistingArchetype()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        _context.Archetypes.Add(archetype);
        _context.SaveChanges();

        var existingArchetypeId = archetype.Id;

        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        // Act
        _playerDao.AddPlayer(player);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        var result = _context.Players
            .Include(p => p.Archetype)
            .Single(p => p.Name == "TestPlayer");

        var archetypeCount = _context.Archetypes
            .Count(a => a.Name == "Warrior");

        // Assert
        Assert.AreEqual(existingArchetypeId, result.ArchetypeId);
        Assert.AreEqual(existingArchetypeId, result.Archetype.Id);
        Assert.AreEqual(1, archetypeCount);
    }

    [TestMethod]
    public void UpdatePlayer_PersistsTrackedPlayerChanges()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        var playerId = player.Id;

        // Retrieve the tracked player.
        var trackedPlayer = _context.Players
            .Single(p => p.Id == playerId);

        // Act
        trackedPlayer.Level = 5;
        trackedPlayer.MaxHealth = 75;

        _playerDao.UpdatePlayer(trackedPlayer);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        var result = _context.Players
            .Single(p => p.Id == playerId);

        // Assert
        Assert.AreEqual(5, result.Level);
        Assert.AreEqual(75, result.MaxHealth);
    }

    [TestMethod]
    public void UpdatePlayer_PersistsTrackedInventoryChanges()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        var playerId = player.Id;

        // Retrieve the tracked player and inventory.
        var trackedPlayer = _context.Players
            .Include(p => p.Inventory)
            .Single(p => p.Id == playerId);

        // Act
        trackedPlayer.Inventory.Gold = 250;
        trackedPlayer.Inventory.Capacity = 20M;

        _playerDao.UpdatePlayer(trackedPlayer);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        var result = _context.Players
            .Include(p => p.Inventory)
            .Single(p => p.Id == playerId);

        // Assert
        Assert.AreEqual(250, result.Inventory.Gold);
        Assert.AreEqual(20M, result.Inventory.Capacity);
    }

    [TestMethod]
    public void UpdatePlayer_PersistsTrackedItemChanges()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("TestPlayer", archetype, inventory);

        var weapon = (Weapon)CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        inventory.Items.Add(weapon);

        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        var weaponId = weapon.Id;

        // Retrieve the tracked player, inventory, and items.
        var trackedPlayer = _context.Players
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Single(p => p.Id == playerId);

        var trackedWeapon = trackedPlayer.Inventory.Items
            .Single(i => i.Id == weaponId);

        // Act
        trackedWeapon.Name = "Enhanced Iron Sword";
        trackedWeapon.Value = 25M;
        trackedWeapon.Durability = 8;

        _playerDao.UpdatePlayer(trackedPlayer);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        var result = _context.Players
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Single(p => p.Id == playerId);

        var resultWeapon = result.Inventory.Items
            .OfType<Weapon>()
            .Single(i => i.Id == weaponId);

        // Assert
        Assert.AreEqual("Enhanced Iron Sword", resultWeapon.Name);
        Assert.AreEqual(25M, resultWeapon.Value);
        Assert.AreEqual(8, resultWeapon.Durability);
    }

    [TestMethod]
    public void DeletePlayer_RemovesPlayerAndAppliesInventoryDeleteBehavior()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);

        var weapon = (Weapon)CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        inventory.Items.Add(weapon);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        var inventoryId = inventory.Id;
        var weaponId = weapon.Id;
        var archetypeId = archetype.Id;

        // Retrieve the tracked player.
        var trackedPlayer = _context.Players
            .Single(p => p.Id == playerId);

        // Act
        _playerDao.DeletePlayer(trackedPlayer);

        // Clear tracked entities to force database verification.
        _context.ChangeTracker.Clear();

        // Assert
        Assert.IsFalse(_context.Players
            .Any(p => p.Id == playerId));

        Assert.IsFalse(_context.Inventories
            .Any(i => i.Id == inventoryId));

        var resultWeapon = _context.Items
            .OfType<Weapon>()
            .SingleOrDefault(i => i.Id == weaponId);

        Assert.IsNotNull(resultWeapon);
        Assert.IsNull(resultWeapon.InventoryId);

        Assert.IsTrue(_context.Archetypes
            .Any(a => a.Id == archetypeId));
    }

    [TestMethod]
    public void DeletePlayer_RemovesInventoryOwnershipRecordsButPreservesDefinitions()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        _context.Archetypes.Add(archetype);
        _context.SaveChanges();

        var rune = CreateRune("Flame Rune", 10, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
        var ingredient = CreateIngredient("Fire Essence", ComponentType.Core);

        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        var inventory = CreateInventory(100);
        inventory.Runes.Add(new InventoryRune
        {
            RuneId = rune.Id,
            Quantity = 2,
            Tier = 1
        });
        inventory.Ingredients.Add(new InventoryIngredient
        {
            IngredientId = ingredient.Id,
            Quantity = 5
        });

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        var inventoryId = inventory.Id;
        var runeId = rune.Id;
        var ingredientId = ingredient.Id;

        // Act
        _playerDao.DeletePlayer(player);

        // Assert
        _context.ChangeTracker.Clear();

        Assert.IsNull(_context.Players.SingleOrDefault(p => p.Id == playerId));
        Assert.IsNull(_context.Inventories.SingleOrDefault(i => i.Id == inventoryId));

        Assert.AreEqual(0, _context.InventoryRunes
            .Count(ir => ir.InventoryId == inventoryId));

        Assert.AreEqual(0, _context.InventoryIngredients
            .Count(ii => ii.InventoryId == inventoryId));

        Assert.IsNotNull(_context.Runes.SingleOrDefault(r => r.Id == runeId));
        Assert.IsNotNull(_context.Ingredients.SingleOrDefault(i => i.Id == ingredientId));
    }

    [TestMethod]
    public void GetPlayerByName_ReturnsPlayerWithInventoryAndItems()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var weapon = (Weapon)CreateItem(
            "Iron Sword", 1, 10, TestItemType.Weapon);

        var armor = (Armor)CreateItem(
            "Iron Armor", 1, 8, TestItemType.Armor);

        inventory.Items.Add(weapon);
        inventory.Items.Add(armor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        // Clear tracked entities to ensure retrieval from the database.
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByName("TestPlayer");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TestPlayer", result.Name);

        Assert.IsNotNull(result.Inventory);
        Assert.AreEqual(150, result.Inventory.Gold);
        Assert.AreEqual(2, result.Inventory.Items.Count);

        Assert.IsTrue(result.Inventory.Items.OfType<Weapon>()
            .Any(i => i.Name == "Iron Sword"));

        Assert.IsTrue(result.Inventory.Items.OfType<Armor>()
            .Any(i => i.Name == "Iron Armor"));
    }

    [TestMethod]
    public void GetPlayerByName_IsCaseInsensitive()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("DragonSlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        // Act
        var result = _playerDao.GetPlayerByName("dRaGoNsLaYeR");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(player.Id, result.Id);
        Assert.AreEqual("DragonSlayer", result.Name);
    }

    [TestMethod]
    public void GetPlayerByName_ReturnsNullWhenPlayerDoesNotExist()
    {
        // Act
        var result = _playerDao.GetPlayerByName("MissingPlayer");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetPlayerByName_RebuildsEquipment()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var equippedWeapon = (Weapon)CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        equippedWeapon.Equip();

        var unequippedWeapon = CreateItem("Wooden Bow", 1, 5, TestItemType.Weapon);

        var equippedArmor = (Armor)CreateItem("Iron Armor", 1, 8, TestItemType.Armor);
        equippedArmor.Equip();

        inventory.Items.Add(equippedWeapon);
        inventory.Items.Add(unequippedWeapon);
        inventory.Items.Add(equippedArmor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByName("TestPlayer");

        // Assert
        Assert.IsNotNull(result);

        var equipment = result.Equipment.OfType<Item>().ToList();

        Assert.AreEqual(2, equipment.Count);
        Assert.IsTrue(equipment.Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(equipment.Any(i => i.Name == "Iron Armor"));
        Assert.IsFalse(equipment.Any(i => i.Name == "Wooden Bow"));
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_ReturnsPlayerWithCompleteGameplayRelationships()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var skill = CreateSkill("Power Strike", TestSkillType.Martial);
        archetype.Skills.Add(skill);

        var inventory = CreateInventory(150);
        var weapon = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        var armor = CreateItem("Iron Armor", 1, 8, TestItemType.Armor);

        inventory.Items.Add(weapon);
        inventory.Items.Add(armor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);

        _playerDao.AddPlayer(player);

        // Add rune and ingredient definitions and ownership records.
        var rune = CreateRune("Flame Rune", 10, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        var ingredient = CreateIngredient("Fire Crystal", ComponentType.MonsterPart);

        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        var inventoryRune = new InventoryRune
        {
            InventoryId = inventory.Id,
            RuneId = rune.Id,
            Tier = 1,
            Quantity = 2
        };

        var inventoryIngredient = new InventoryIngredient
        {
            InventoryId = inventory.Id,
            IngredientId = ingredient.Id,
            Quantity = 3
        };

        _context.InventoryRunes.Add(inventoryRune);
        _context.InventoryIngredients.Add(inventoryIngredient);
        _context.SaveChanges();

        var playerId = player.Id;

        // Clear tracked entities to force a database reload.
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);

        // Inventory and items
        Assert.AreEqual(150, result.Inventory.Gold);
        Assert.AreEqual(2, result.Inventory.Items.Count);
        Assert.IsTrue(result.Inventory.Items.OfType<Weapon>()
            .Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(result.Inventory.Items.OfType<Armor>()
            .Any(i => i.Name == "Iron Armor"));

        // Runes and their definitions
        Assert.AreEqual(1, result.Inventory.Runes.Count);
        Assert.AreEqual("Flame Rune", result.Inventory.Runes.Single().Rune.Name);

        // Ingredients and their definitions
        Assert.AreEqual(1, result.Inventory.Ingredients.Count);
        Assert.AreEqual("Fire Crystal", result.Inventory.Ingredients.Single().Ingredient.Name);

        // Archetype and skills
        Assert.AreEqual("Warrior", result.Archetype.Name);
        Assert.AreEqual(1, result.Archetype.Skills.Count);
        Assert.AreEqual("Power Strike", result.Archetype.Skills.Single().Name);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_ReturnsDetachedPlayerGraph()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);
        var weapon = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);

        inventory.Items.Add(weapon);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(EntityState.Detached, _context.Entry(result).State);
        Assert.AreEqual(EntityState.Detached, _context.Entry(result.Inventory).State);
        Assert.AreEqual(EntityState.Detached, _context.Entry(result.Archetype).State);
        Assert.AreEqual(EntityState.Detached, _context.Entry(result.Inventory.Items.Single()).State);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_ReturnsNullWhenPlayerDoesNotExist()
    {
        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(7);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_RebuildsEquipment()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var equippedWeapon = (Weapon)CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        equippedWeapon.Equip();

        var unequippedWeapon = CreateItem("Wooden Bow", 1, 5, TestItemType.Weapon);

        var equippedArmor = (Armor)CreateItem("Iron Armor", 1, 8, TestItemType.Armor);
        equippedArmor.Equip();

        inventory.Items.Add(equippedWeapon);
        inventory.Items.Add(unequippedWeapon);
        inventory.Items.Add(equippedArmor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);

        var equipment = result.Equipment.OfType<Item>().ToList();

        Assert.AreEqual(2, equipment.Count);
        Assert.IsTrue(equipment.Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(equipment.Any(i => i.Name == "Iron Armor"));
        Assert.IsFalse(equipment.Any(i => i.Name == "Wooden Bow"));
    }

    [TestMethod]
    public void GetAllPlayers_ReturnsAllPlayers()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var player1 = CreatePlayer("PlayerOne", archetype, CreateInventory(100));
        var player2 = CreatePlayer("PlayerTwo", archetype, CreateInventory(200));
        var player3 = CreatePlayer("PlayerThree", archetype, CreateInventory(300));

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);
        _playerDao.AddPlayer(player3);

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetAllPlayers();

        // Assert
        Assert.AreEqual(3, result.Count);

        Assert.IsTrue(result.Any(p => p.Name == "PlayerOne"));
        Assert.IsTrue(result.Any(p => p.Name == "PlayerTwo"));
        Assert.IsTrue(result.Any(p => p.Name == "PlayerThree"));
    }

    [TestMethod]
    public void GetAllPlayers_ReturnsEmptyListWhenNoPlayersExist()
    {
        // Act
        var resultList = _playerDao.GetAllPlayers();

        // Assert
        Assert.IsNotNull(resultList);
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public void GetAllPlayersByName_ReturnsMatchingPlayers()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var player1 = CreatePlayer("DragonSlayer", archetype, CreateInventory(100));
        var player2 = CreatePlayer("EvilDragonKnight", archetype, CreateInventory(200));
        var player3 = CreatePlayer("ShadowMage", archetype, CreateInventory(300));

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);
        _playerDao.AddPlayer(player3);

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetAllPlayers("Dragon");

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.Any(p => p.Name == "DragonSlayer"));
        Assert.IsTrue(result.Any(p => p.Name == "EvilDragonKnight"));
        Assert.IsFalse(result.Any(p => p.Name == "ShadowMage"));
    }

    [TestMethod]
    public void GetAllPlayersByName_IsCaseInsensitive()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var player1 = CreatePlayer("DragonSlayer", archetype, CreateInventory(100));
        var player2 = CreatePlayer("EvilDragonKnight", archetype, CreateInventory(200));

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetAllPlayers("dRaGoN");

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.Any(p => p.Name == "DragonSlayer"));
        Assert.IsTrue(result.Any(p => p.Name == "EvilDragonKnight"));
    }

    [TestMethod]
    public void GetAllPlayersByName_ReturnsEmptyListWhenNoMatchesExist()
    {
        // Act
        var resultList = _playerDao.GetAllPlayers("Test");

        // Assert
        Assert.IsNotNull(resultList);
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public void GetDistinctPlayerArchetypes_ReturnsOnlyArchetypesUsedByPlayers()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Martial);
        var mage = CreateArchetype("Mage", ArchetypeType.Magical);

        var player1 = CreatePlayer("PlayerOne", warrior, CreateInventory(100));
        var player2 = CreatePlayer("PlayerTwo", warrior, CreateInventory(200));
        var player3 = CreatePlayer("PlayerThree", mage, CreateInventory(300));

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);
        _playerDao.AddPlayer(player3);

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetDistinctPlayerArchetypes();

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetDistinctPlayerArchetypes_ReturnsEmptyListWhenNoPlayersExist()
    {
        // Act
        var resultList = _playerDao.GetDistinctPlayerArchetypes();

        // Assert
        Assert.IsNotNull(resultList);
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public void GetPlayersByArchetypeId_ReturnsMatchingPlayers()
    {
        // Arrange
        var warrior = CreateArchetype("Warrior", ArchetypeType.Martial);
        var mage = CreateArchetype("Mage", ArchetypeType.Magical);

        var player1 = CreatePlayer("PlayerOne", warrior, CreateInventory(100));
        var player2 = CreatePlayer("PlayerTwo", warrior, CreateInventory(200));
        var player3 = CreatePlayer("PlayerThree", mage, CreateInventory(300));

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);
        _playerDao.AddPlayer(player3);

        var warriorId = warrior.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayersByArchetypeId(warriorId);

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.Any(p => p.Name == "PlayerOne"));
        Assert.IsTrue(result.Any(p => p.Name == "PlayerTwo"));
        Assert.IsFalse(result.Any(p => p.Name == "PlayerThree"));
    }

    [TestMethod]
    public void GetPlayersByArchetypeId_ReturnsEmptyListWhenNoMatchesExist()
    {
        // Act
        var resultList = _playerDao.GetPlayersByArchetypeId(999);

        // Assert
        Assert.IsNotNull(resultList);
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public void GetPlayersByLevel_ReturnsMatchingPlayers()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);

        var player1 = CreatePlayer("PlayerOne", archetype, CreateInventory(100));
        var player2 = CreatePlayer("PlayerTwo", archetype, CreateInventory(200));
        var player3 = CreatePlayer("PlayerThree", archetype, CreateInventory(300));

        player1.Level = 5;
        player2.Level = 5;
        player3.Level = 10;

        _playerDao.AddPlayer(player1);
        _playerDao.AddPlayer(player2);
        _playerDao.AddPlayer(player3);

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayersByLevel(5);

        // Assert
        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.Any(p => p.Name == "PlayerOne"));
        Assert.IsTrue(result.Any(p => p.Name == "PlayerTwo"));
        Assert.IsFalse(result.Any(p => p.Name == "PlayerThree"));
    }

    [TestMethod]
    public void GetPlayersByLevel_ReturnsEmptyListWhenNoMatchesExist()
    {
        // Act
        var resultList = _playerDao.GetPlayersByLevel(5);

        // Assert
        Assert.IsNotNull(resultList);
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public void DetachedArchetype_ReconstructsProgressionWithoutChangingDatabase()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var player = CreatePlayer("TestPlayer", archetype, CreateInventory(150));

        player.Level = 5;
        player.MaxHealth = 150;

        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        var archetypeId = archetype.Id;

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        Assert.IsNotNull(result);

        // Verify persisted values before reconstruction
        Assert.AreEqual(5, result.Level);
        Assert.AreEqual(150, result.MaxHealth);

        // Reconstruct progression for the detached player
        for (var i = 2; i <= result.Level; i++)
            result.Archetype.LevelUp(i);

        // Assert: detached archetype has progressed
        Assert.AreNotEqual(archetype.AttackBonus, result.Archetype.AttackBonus);
        Assert.AreNotEqual(archetype.DefenseBonus, result.Archetype.DefenseBonus);

        // Assert: database archetype remains unchanged
        _context.ChangeTracker.Clear();

        var savedArchetype = _context.Archetypes
            .AsNoTracking()
            .Single(a => a.Id == archetypeId);

        Assert.AreEqual(archetype.AttackBonus, savedArchetype.AttackBonus);
        Assert.AreEqual(archetype.MagicBonus, savedArchetype.MagicBonus);
        Assert.AreEqual(archetype.DefenseBonus, savedArchetype.DefenseBonus);
        Assert.AreEqual(archetype.ResistanceBonus, savedArchetype.ResistanceBonus);
        Assert.AreEqual(archetype.Speed, savedArchetype.Speed);
        Assert.AreEqual(archetype.MaxResource, savedArchetype.MaxResource);
        Assert.AreEqual(archetype.RecoveryRate, savedArchetype.RecoveryRate);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_PreservesPersistedPlayerLevelAndMaxHealth()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var player = CreatePlayer("TestPlayer", archetype, CreateInventory(150));

        player.Level = 10;
        player.MaxHealth = 250;

        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10, result.Level);
        Assert.AreEqual(250, result.MaxHealth);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_PreservesEquippedWeaponAndArmor()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var equippedWeapon = (Weapon)CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        equippedWeapon.Equip();

        var unequippedWeapon = CreateItem("Wooden Bow", 1, 5, TestItemType.Weapon);

        var equippedArmor = (Armor)CreateItem("Iron Armor", 1, 8, TestItemType.Armor);
        equippedArmor.Equip();

        inventory.Items.Add(equippedWeapon);
        inventory.Items.Add(unequippedWeapon);
        inventory.Items.Add(equippedArmor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);

        var equippableWeapons = result.Inventory.Items.OfType<Weapon>().ToList();
        var weapon = equippableWeapons.Single(i => i.Name == "Iron Sword");
        var unequipped = equippableWeapons.Single(i => i.Name == "Wooden Bow");

        var armor = result.Inventory.Items.OfType<Armor>()
            .Single(i => i.Name == "Iron Armor");

        Assert.IsTrue(weapon.IsEquipped());
        Assert.IsTrue(armor.IsEquipped());
        Assert.IsFalse(unequipped.IsEquipped());
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_DoesNotModifyDatabaseWhenGameplayStateChanges()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);
        var weapon = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);

        inventory.Items.Add(weapon);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        player.Level = 5;
        player.MaxHealth = 150;

        _playerDao.AddPlayer(player);

        var playerId = player.Id;
        var weaponId = weapon.Id;

        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        Assert.IsNotNull(result);

        // Modify gameplay state
        result.Level = 6;
        result.MaxHealth = 200;
        result.CurrentHealth = 50;
        result.Inventory.Gold = 500;

        var loadedWeapon = result.Inventory.Items
            .Single(i => i.Id == weaponId);

        loadedWeapon.Name = "Legendary Sword";

        // Assert: database remains unchanged
        _context.ChangeTracker.Clear();

        var savedPlayer = _context.Players
            .AsNoTracking()
            .Include(p => p.Inventory)
            .ThenInclude(i => i.Items)
            .Single(p => p.Id == playerId);

        Assert.AreEqual(5, savedPlayer.Level);
        Assert.AreEqual(150, savedPlayer.MaxHealth);
        Assert.AreEqual(150, savedPlayer.Inventory.Gold);

        var savedWeapon = savedPlayer.Inventory.Items
            .Single(i => i.Id == weaponId);

        Assert.AreEqual("Iron Sword", savedWeapon.Name);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_LoadsInventoryRunesWithDefinitions()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var rune = CreateRune("Flame Rune", 10, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);

        _context.Runes.Add(rune);
        _context.SaveChanges();

        var inventoryRune = new InventoryRune
        {
            InventoryId = inventory.Id,
            RuneId = rune.Id,
            Tier = 2,
            Quantity = 3
        };

        _context.InventoryRunes.Add(inventoryRune);
        _context.SaveChanges();

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Inventory.Runes.Count);

        var ownedRune = result.Inventory.Runes.Single();

        Assert.AreEqual(2, ownedRune.Tier);
        Assert.AreEqual(3, ownedRune.Quantity);
        Assert.AreEqual("Flame Rune", ownedRune.Rune.Name);
        Assert.AreEqual(ElementType.Fire, ownedRune.Rune.Element);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_LoadsInventoryIngredientsWithDefinitions()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(150);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var ingredient = CreateIngredient("Fire Crystal", ComponentType.Core);

        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        var inventoryIngredient = new InventoryIngredient
        {
            InventoryId = inventory.Id,
            IngredientId = ingredient.Id,
            Quantity = 3
        };

        _context.InventoryIngredients.Add(inventoryIngredient);
        _context.SaveChanges();

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Inventory.Ingredients.Count);

        var ownedIngredient = result.Inventory.Ingredients.Single();

        Assert.AreEqual(3, ownedIngredient.Quantity);
        Assert.IsNotNull(ownedIngredient.Ingredient);
        Assert.AreEqual("Fire Crystal", ownedIngredient.Ingredient.Name);
        Assert.AreEqual(ComponentType.Core, ownedIngredient.Ingredient.ComponentType);
    }

    [TestMethod]
    public void GetPlayerByIdNoTracking_LoadsCompleteInventory()
    {
        // Arrange
        var archetype = CreateArchetype("Warrior", ArchetypeType.Martial);
        var inventory = CreateInventory(500);
        inventory.Capacity = 20;

        var weapon = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        var armor = CreateItem("Iron Armor", 1, 8, TestItemType.Armor);

        inventory.Items.Add(weapon);
        inventory.Items.Add(armor);

        var player = CreatePlayer("TestPlayer", archetype, inventory);
        _playerDao.AddPlayer(player);

        var rune = CreateRune("Flame Rune", 10, ElementType.Fire, RarityLevel.Common, TestRuneType.Weapon);
        var ingredient = CreateIngredient("Fire Crystal", ComponentType.Core);

        _context.Runes.Add(rune);
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();

        _context.InventoryRunes.Add(new InventoryRune
        {
            InventoryId = inventory.Id,
            RuneId = rune.Id,
            Tier = 2,
            Quantity = 3
        });

        _context.InventoryIngredients.Add(new InventoryIngredient
        {
            InventoryId = inventory.Id,
            IngredientId = ingredient.Id,
            Quantity = 5
        });

        _context.SaveChanges();

        var playerId = player.Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = _playerDao.GetPlayerByIdNoTracking(playerId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Inventory);

        // Gold and capacity
        Assert.AreEqual(500, result.Inventory.Gold);
        Assert.AreEqual(20, result.Inventory.Capacity);

        // Items
        Assert.AreEqual(2, result.Inventory.Items.Count);
        Assert.IsTrue(result.Inventory.Items.Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(result.Inventory.Items.Any(i => i.Name == "Iron Armor"));

        // Runes
        Assert.AreEqual(1, result.Inventory.Runes.Count);
        Assert.AreEqual("Flame Rune", result.Inventory.Runes.Single().Rune.Name);
        Assert.AreEqual(3, result.Inventory.Runes.Single().Quantity);

        // Ingredients
        Assert.AreEqual(1, result.Inventory.Ingredients.Count);
        Assert.AreEqual("Fire Crystal", result.Inventory.Ingredients.Single().Ingredient.Name);
        Assert.AreEqual(5, result.Inventory.Ingredients.Single().Quantity);
    }
}