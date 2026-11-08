using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class ItemDaoTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private ItemDao _itemDao = null!;

    private static readonly string[] AscendingSwordNames =
    [
        "Bronze Sword",
        "Copper Sword",
        "Iron Sword"
    ];

    private static readonly string[] DescendingSwordNames =
    [
        "Iron Sword",
        "Copper Sword",
        "Bronze Sword"
    ];

    private static readonly string[] AllSwordNames =
    [
        "Iron Sword",
        "Bronze Sword",
        "Copper Sword"
    ];

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
        _itemDao = new ItemDao(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void AddItem_PersistsItem()
    {
        // Arrange
        var item = CreateItem("Test Item", 1, 10, TestItemType.Weapon);
        var dao = new ItemDao(_context);

        // Act
        dao.AddItem(item);

        _context.ChangeTracker.Clear();

        var result = _context.Items!.Single(i => i.Name == "Test Item");

        // Assert
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual("Test Item", result.Name);
        Assert.AreEqual("A test weapon.", result.Description);
        Assert.AreEqual(1, result.RequiredLevel);
        Assert.AreEqual(4.6M, result.Value);
        Assert.AreEqual(3, result.Durability);
        Assert.IsFalse(result.IsCore);
    }

    [TestMethod]
    public void AddItem_PersistsWeaponWithWeaponProperties()
    {
        // Arrange
        var weapon = CreateItem("Test Weapon", 2, 15, TestItemType.Weapon);

        // Act
        _itemDao.AddItem(weapon);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Weapon>()
            .Single(i => i.Name == "Test Weapon");

        // Assert
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual("Test Weapon", result.Name);
        Assert.AreEqual("A test weapon.", result.Description);
        Assert.AreEqual(2, result.RequiredLevel);
        Assert.AreEqual(4.6M, result.Value);
        Assert.AreEqual(3, result.Durability);
        Assert.AreEqual(15, result.AttackPower);
        Assert.AreEqual(DamageType.Martial, result.DamageType);
    }

    [TestMethod]
    public void AddItem_PersistsArmorWithArmorProperties()
    {
        // Arrange
        var armor = CreateItem("Test Armor", 3, 20, TestItemType.Armor, ArmorType.Head);

        // Act
        _itemDao.AddItem(armor);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Armor>()
            .Single(i => i.Name == "Test Armor");

        // Assert
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual("Test Armor", result.Name);
        Assert.AreEqual("A test armor.", result.Description);
        Assert.AreEqual(3, result.RequiredLevel);
        Assert.AreEqual(4.6M, result.Value);
        Assert.AreEqual(3, result.Durability);
        Assert.AreEqual(20, result.DefensePower);
        Assert.AreEqual(ArmorType.Head, result.ArmorType);
    }

    [TestMethod]
    public void AddItem_PersistsConsumableWithConsumableProperties()
    {
        // Arrange
        var consumable = CreateItem("Test Consumable", 1, 25, TestItemType.Consumable);

        var dao = new ItemDao(_context);

        // Act
        dao.AddItem(consumable);

        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Consumable>()
            .Single(i => i.Name == "Test Consumable");

        // Assert
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual("Test Consumable", result.Name);
        Assert.AreEqual("A test consumable.", result.Description);
        Assert.AreEqual(1, result.RequiredLevel);
        Assert.AreEqual(4.6M, result.Value);
        Assert.AreEqual(3, result.Durability);
        Assert.AreEqual(25, result.Power);
        Assert.AreEqual(ConsumableType.Health, result.ConsumableType);
    }

    [TestMethod]
    public void AddItem_PersistsValuableWithValuableProperties()
    {
        // Arrange
        var valuable = CreateItem("Test Valuable", 1, 50, TestItemType.Valuable);

        // Act
        _itemDao.AddItem(valuable);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Valuable>()
            .Single(i => i.Name == "Test Valuable");

        // Assert
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual("Test Valuable", result.Name);
        Assert.AreEqual("A test valuable.", result.Description);
        Assert.AreEqual(1, result.RequiredLevel);
        Assert.AreEqual(4.6M, result.Value);
        Assert.AreEqual(3, result.Durability);
    }

    [TestMethod]
    public void UpdateItem_PersistsItemChanges()
    {
        // Arrange
        var item = CreateItem("Test Item", 1, 10, TestItemType.Weapon);
        _itemDao.AddItem(item);
        _context.SaveChanges();

        item.Name = "Updated Item";
        item.Description = "Updated description.";
        item.RequiredLevel = 5;
        item.Value = 12.5M;
        item.Durability = 8;

        // Act
        _itemDao.UpdateItem(item);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.Single(i => i.Id == item.Id);

        // Assert
        Assert.AreEqual("Updated Item", result.Name);
        Assert.AreEqual("Updated description.", result.Description);
        Assert.AreEqual(5, result.RequiredLevel);
        Assert.AreEqual(12.5M, result.Value);
        Assert.AreEqual(8, result.Durability);
    }

    [TestMethod]
    public void UpdateItem_PersistsWeaponPropertyChanges()
    {
        // Arrange
        var weapon = (Weapon)CreateItem("Test Weapon", 1, 10, TestItemType.Weapon);

        _itemDao.AddItem(weapon);
        _context.SaveChanges();

        weapon.AttackPower = 25;
        weapon.DamageType = DamageType.Magical;
        weapon.Durability = 10;

        // Act
        _itemDao.UpdateItem(weapon);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Weapon>()
            .Single(i => i.Id == weapon.Id);

        // Assert
        Assert.AreEqual(25, result.AttackPower);
        Assert.AreEqual(DamageType.Magical, result.DamageType);
        Assert.AreEqual(10, result.Durability);
    }

    [TestMethod]
    public void UpdateItem_PersistsArmorPropertyChanges()
    {
        // Arrange
        var armor = (Armor)CreateItem("Test Armor", 1, 10, TestItemType.Armor);

        _itemDao.AddItem(armor);
        _context.SaveChanges();

        armor.DefensePower = 30;
        armor.ArmorType = ArmorType.Chest;
        armor.Durability = 12;

        // Act
        _itemDao.UpdateItem(armor);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Armor>()
            .Single(i => i.Id == armor.Id);

        // Assert
        Assert.AreEqual(30, result.DefensePower);
        Assert.AreEqual(ArmorType.Chest, result.ArmorType);
        Assert.AreEqual(12, result.Durability);
    }

    [TestMethod]
    public void UpdateItem_PersistsConsumablePropertyChanges()
    {
        // Arrange
        var consumable = (Consumable)CreateItem("Test Consumable", 1, 10, TestItemType.Consumable);

        _itemDao.AddItem(consumable);
        _context.SaveChanges();

        consumable.Power = 30;
        consumable.ConsumableType = ConsumableType.Durability;
        consumable.Durability = 5;

        // Act
        _itemDao.UpdateItem(consumable);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Consumable>()
            .Single(i => i.Id == consumable.Id);

        // Assert
        Assert.AreEqual(30, result.Power);
        Assert.AreEqual(ConsumableType.Durability, result.ConsumableType);
        Assert.AreEqual(5, result.Durability);
    }

    [TestMethod]
    public void UpdateItem_PersistsValuablePropertyChanges()
    {
        // Arrange
        var valuable = CreateItem("Test Valuable", 1, 10, TestItemType.Valuable);

        _itemDao.AddItem(valuable);
        _context.SaveChanges();

        valuable.Value = 25.5M;
        valuable.Name = "Updated Valuable";
        valuable.Description = "Updated valuable description.";

        // Act
        _itemDao.UpdateItem(valuable);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.OfType<Valuable>()
            .Single(i => i.Id == valuable.Id);

        // Assert
        Assert.AreEqual("Updated Valuable", result.Name);
        Assert.AreEqual("Updated valuable description.", result.Description);
        Assert.AreEqual(25.5M, result.Value);
    }

    [TestMethod]
    public void UpdateItem_PersistsIsCoreChanges()
    {
        // Arrange
        var item = CreateItem("Test Item", 1, 10, TestItemType.Valuable);
        _itemDao.AddItem(item);
        _context.SaveChanges();

        Assert.IsFalse(item.IsCore);

        item.IsCore = true;

        // Act
        _itemDao.UpdateItem(item);
        _context.ChangeTracker.Clear();

        var result = _context.Items!.Single(i => i.Id == item.Id);

        // Assert
        Assert.IsTrue(result.IsCore);
    }

    [TestMethod]
    public void DeleteItem_RemovesNonCoreItem()
    {
        // Arrange
        var item = CreateItem("Test Item", 1, 10, TestItemType.Valuable);
        _itemDao.AddItem(item);

        var itemId = item.Id;

        // Act
        _itemDao.DeleteItem(item);

        // Assert
        Assert.IsFalse(_context.Items!.Any(i => i.Id == itemId));
    }

    [TestMethod]
    public void DeleteItem_ThrowsForCoreItemAndPreservesIt()
    {
        // Arrange
        var item = CreateItem("Core Item", 1, 10, TestItemType.Valuable);
        item.IsCore = true;

        _itemDao.AddItem(item);

        var itemId = item.Id;

        // Act & Assert
        var exception = Assert.ThrowsException<InvalidOperationException>(() => _itemDao.DeleteItem(item));

        Assert.AreEqual("Core game items cannot be deleted.", exception.Message);

        _context.ChangeTracker.Clear();

        var result = _context.Items!.SingleOrDefault(i => i.Id == itemId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsCore);
        Assert.AreEqual("Core Item", result.Name);
    }

    [TestMethod]
    public void GetItemsByName_ReturnsMatchingItems()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Rusty Iron Shield", 1, 10, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Health Potion", 1, 10, TestItemType.Consumable));

        // Act
        var result = _itemDao.GetItemsByName("Iron");

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(i => i.Name.Contains("Iron")));
    }

    [TestMethod]
    public void GetItemsByName_IsCaseInsensitive()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsByName("iRoN");

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Iron Sword", result[0].Name);
    }

    [TestMethod]
    public void GetItemsByName_ReturnsEmptyListWhenNoMatchesExist()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsByName("Dragon");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetItemsByName_IncludesInventoryAndPlayer()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Magical);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);

        var item = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        inventory.AddItem(item);

        _context.Players!.Add(player);
        _context.SaveChanges();

        _context.ChangeTracker.Clear();

        // Act
        var result = _itemDao.GetItemsByName("Iron Sword");

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].Inventory);
        Assert.AreEqual("Test Player", result[0].Inventory!.Player.Name);
    }

    [TestMethod]
    public void GetItemsByName_SortsAscendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetItemsByName("Sword");

        // Assert
        CollectionAssert.AreEqual(
            AscendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetItemsByName_SortsDescendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetItemsByName("Sword");

        // Assert
        CollectionAssert.AreEqual(
            DescendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetItemsByName_ReturnsUnsortedResultsForInvalidSortOrder()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "INVALID";

        // Act
        var result = _itemDao.GetItemsByName("Sword");

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(
            AllSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllItems_ReturnsAllItems()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Leather Armor", 1, 10, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Health Potion", 1, 10, TestItemType.Consumable));

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(result.Any(i => i.Name == "Leather Armor"));
        Assert.IsTrue(result.Any(i => i.Name == "Health Potion"));
    }

    [TestMethod]
    public void GetAllItems_IncludesInventoryAndPlayer()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var item = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);

        inventory.AddItem(item);
        _context.Players!.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].Inventory);
        Assert.AreEqual("Test Player", result[0].Inventory!.Player.Name);
    }

    [TestMethod]
    public void GetAllItems_SortsAscendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        CollectionAssert.AreEqual(
            AscendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllItems_SortsDescendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        CollectionAssert.AreEqual(
            DescendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllItems_ReturnsUnsortedResultsForInvalidSortOrder()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "INVALID";

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(
            AllSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllItems_ReturnsEmptyListWhenNoItemsExist()
    {
        // Arrange

        // Act
        var result = _itemDao.GetAllItems();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAllNonCoreItems_ReturnsOnlyNonCoreItems()
    {
        // Arrange
        var nonCoreItem1 = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);
        var nonCoreItem2 = CreateItem("Health Potion", 1, 10, TestItemType.Consumable);
        var coreItem = CreateItem("Core Sword", 1, 10, TestItemType.Weapon);
        coreItem.IsCore = true;

        _itemDao.AddItem(nonCoreItem1);
        _itemDao.AddItem(nonCoreItem2);
        _itemDao.AddItem(coreItem);

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(i => !i.IsCore));
        Assert.IsTrue(result.Any(i => i.Name == "Iron Sword"));
        Assert.IsTrue(result.Any(i => i.Name == "Health Potion"));
    }

    [TestMethod]
    public void GetAllNonCoreItems_IncludesInventoryAndPlayer()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Magical);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var item = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);

        inventory.AddItem(item);
        _context.Players!.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].Inventory);
        Assert.AreEqual("Test Player", result[0].Inventory!.Player.Name);
    }

    [TestMethod]
    public void GetAllNonCoreItems_SortsAscendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        CollectionAssert.AreEqual(
            AscendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllNonCoreItems_SortsDescendingByName()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        CollectionAssert.AreEqual(
            DescendingSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllNonCoreItems_ReturnsUnsortedResultsForInvalidSortOrder()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Copper Sword", 1, 10, TestItemType.Weapon));

        _itemDao.SortOrder = "INVALID";

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(
            AllSwordNames,
            result.Select(i => i.Name).ToArray());
    }

    [TestMethod]
    public void GetAllNonCoreItems_ExcludesCoreItemsAndReturnsEmptyWhenNoNonCoreItemsExist()
    {
        // Arrange
        var coreItem1 = CreateItem("Core Sword", 1, 10, TestItemType.Weapon);
        var coreItem2 = CreateItem("Core Armor", 1, 10, TestItemType.Armor);

        coreItem1.IsCore = true;
        coreItem2.IsCore = true;

        _itemDao.AddItem(coreItem1);
        _itemDao.AddItem(coreItem2);

        // Act
        var result = _itemDao.GetAllNonCoreItems();

        // Assert
        Assert.AreEqual(0, result.Count);
        Assert.IsTrue(result.All(i => !i.IsCore));
    }

    [TestMethod]
    public void GetItemsByType_ReturnsWeapons()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Health Potion", 1, 10, TestItemType.Consumable));

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Weapon);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.All(i => i is Weapon));
    }

    [TestMethod]
    public void GetItemsByType_ReturnsArmor()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Armor", 1, 10, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Armor);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.All(i => i is Armor));
    }

    [TestMethod]
    public void GetItemsByType_ReturnsValuables()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Gold Ring", 1, 10, TestItemType.Valuable));
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Valuable);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.All(i => i is Valuable));
    }

    [TestMethod]
    public void GetItemsByType_ReturnsConsumables()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Health Potion", 1, 10, TestItemType.Consumable));
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Consumable);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.All(i => i is Consumable));
    }

    [TestMethod]
    public void GetItemsByType_SortsWeaponsByAttackPowerAscending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 30, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Steel Sword", 1, 20, TestItemType.Weapon));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Weapon);

        // Assert
        Assert.AreEqual(10, ((Weapon)result[0]).AttackPower);
        Assert.AreEqual(20, ((Weapon)result[1]).AttackPower);
        Assert.AreEqual(30, ((Weapon)result[2]).AttackPower);
    }

    [TestMethod]
    public void GetItemsByType_SortsWeaponsByAttackPowerDescending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 30, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Steel Sword", 1, 20, TestItemType.Weapon));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Weapon);

        // Assert
        Assert.AreEqual(30, ((Weapon)result[0]).AttackPower);
        Assert.AreEqual(20, ((Weapon)result[1]).AttackPower);
        Assert.AreEqual(10, ((Weapon)result[2]).AttackPower);
    }

    [TestMethod]
    public void GetItemsByType_SortsArmorByDefensePowerAscending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Armor", 1, 30, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Bronze Armor", 1, 10, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Steel Armor", 1, 20, TestItemType.Armor));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Armor);

        // Assert
        Assert.AreEqual(10, ((Armor)result[0]).DefensePower);
        Assert.AreEqual(20, ((Armor)result[1]).DefensePower);
        Assert.AreEqual(30, ((Armor)result[2]).DefensePower);
    }

    [TestMethod]
    public void GetItemsByType_SortsArmorByDefensePowerDescending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Armor", 1, 30, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Bronze Armor", 1, 10, TestItemType.Armor));
        _itemDao.AddItem(CreateItem("Steel Armor", 1, 20, TestItemType.Armor));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Armor);

        // Assert
        Assert.AreEqual(30, ((Armor)result[0]).DefensePower);
        Assert.AreEqual(20, ((Armor)result[1]).DefensePower);
        Assert.AreEqual(10, ((Armor)result[2]).DefensePower);
    }

    [TestMethod]
    public void GetItemsByType_SortsValuablesByValueAscending()
    {
        // Arrange
        var valuable1 = CreateItem("Gold Ring", 1, 10, TestItemType.Valuable);
        var valuable2 = CreateItem("Silver Ring", 1, 10, TestItemType.Valuable);
        var valuable3 = CreateItem("Bronze Ring", 1, 10, TestItemType.Valuable);

        valuable1.Value = 30M;
        valuable2.Value = 10M;
        valuable3.Value = 20M;

        _itemDao.AddItem(valuable1);
        _itemDao.AddItem(valuable2);
        _itemDao.AddItem(valuable3);

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Valuable);

        // Assert
        Assert.AreEqual(10M, result[0].Value);
        Assert.AreEqual(20M, result[1].Value);
        Assert.AreEqual(30M, result[2].Value);
    }

    [TestMethod]
    public void GetItemsByType_SortsValuablesByValueDescending()
    {
        // Arrange
        var valuable1 = CreateItem("Gold Ring", 1, 10, TestItemType.Valuable);
        var valuable2 = CreateItem("Silver Ring", 1, 10, TestItemType.Valuable);
        var valuable3 = CreateItem("Bronze Ring", 1, 10, TestItemType.Valuable);

        valuable1.Value = 30M;
        valuable2.Value = 10M;
        valuable3.Value = 20M;

        _itemDao.AddItem(valuable1);
        _itemDao.AddItem(valuable2);
        _itemDao.AddItem(valuable3);

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Valuable);

        // Assert
        Assert.AreEqual(30M, result[0].Value);
        Assert.AreEqual(20M, result[1].Value);
        Assert.AreEqual(10M, result[2].Value);
    }

    [TestMethod]
    public void GetItemsByType_SortsConsumablesByPowerAscending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Strong Potion", 1, 30, TestItemType.Consumable));
        _itemDao.AddItem(CreateItem("Weak Potion", 1, 10, TestItemType.Consumable));
        _itemDao.AddItem(CreateItem("Medium Potion", 1, 20, TestItemType.Consumable));

        _itemDao.SortOrder = "ASC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Consumable);

        // Assert
        Assert.AreEqual(10, ((Consumable)result[0]).Power);
        Assert.AreEqual(20, ((Consumable)result[1]).Power);
        Assert.AreEqual(30, ((Consumable)result[2]).Power);
    }

    [TestMethod]
    public void GetItemsByType_SortsConsumablesByPowerDescending()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Strong Potion", 1, 30, TestItemType.Consumable));
        _itemDao.AddItem(CreateItem("Weak Potion", 1, 10, TestItemType.Consumable));
        _itemDao.AddItem(CreateItem("Medium Potion", 1, 20, TestItemType.Consumable));

        _itemDao.SortOrder = "DESC";

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Consumable);

        // Assert
        Assert.AreEqual(30, ((Consumable)result[0]).Power);
        Assert.AreEqual(20, ((Consumable)result[1]).Power);
        Assert.AreEqual(10, ((Consumable)result[2]).Power);
    }

    [TestMethod]
    public void GetItemsByType_ReturnsEmptyListForInvalidType()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Iron Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Iron Armor", 1, 10, TestItemType.Armor));

        // Act
        var result = _itemDao.GetItemsByType((ItemType)999);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetItemsByType_IncludesInventoryAndPlayer()
    {
        // Arrange
        var archetype = CreateArchetype("Test Archetype", ArchetypeType.Martial);
        var inventory = CreateInventory(100);
        var player = CreatePlayer("Test Player", archetype, inventory);
        var item = CreateItem("Iron Sword", 1, 10, TestItemType.Weapon);

        inventory.AddItem(item);
        _context.Players!.Add(player);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        // Act
        var result = _itemDao.GetItemsByType(ItemType.Weapon);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].Inventory);
        Assert.AreEqual("Test Player", result[0].Inventory!.Player.Name);
    }

    [TestMethod]
    public void GetItemsAvailableAtLevel_ReturnsItemsAtOrBelowLevel()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Iron Sword", 5, 20, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Steel Sword", 10, 30, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsAvailableAtLevel(5);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(i => i.RequiredLevel <= 5));
        Assert.IsTrue(result.Any(i => i.Name == "Bronze Sword"));
        Assert.IsTrue(result.Any(i => i.Name == "Iron Sword"));
    }

    [TestMethod]
    public void GetItemsAvailableAtLevel_ExcludesItemsAboveLevel()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Bronze Sword", 1, 10, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Iron Sword", 5, 20, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Steel Sword", 10, 30, TestItemType.Weapon));

        // Act
        var result = _itemDao.GetItemsAvailableAtLevel(5);

        // Assert
        Assert.IsFalse(result.Any(i => i.RequiredLevel > 5));
        Assert.IsFalse(result.Any(i => i.Name == "Steel Sword"));
    }

    [TestMethod]
    public void GetItemsAvailableAtLevel_ReturnsEmptyListWhenNoItemsQualify()
    {
        // Arrange
        _itemDao.AddItem(CreateItem("Steel Sword", 10, 30, TestItemType.Weapon));
        _itemDao.AddItem(CreateItem("Dragon Armor", 15, 50, TestItemType.Armor));

        // Act
        var result = _itemDao.GetItemsAvailableAtLevel(5);

        // Assert
        Assert.AreEqual(0, result.Count);
    }
}