using ConsoleGame.GameDao;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameTests.TestHelpers.ModelCreationHelper;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Integration.ConsoleGame.GameDao;

[TestClass]
public class EquipmentPersistenceTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;

    private ItemDao _itemDao = null!;

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
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void EquippedWeapon_RemainsEquippedAfterDatabaseReload()
    {
        var weapon = CreateItem("Sword", 1, 5, TestItemType.Weapon) as Weapon;
        weapon!.Equip();
        _itemDao.UpdateItem(weapon);
        _context.ChangeTracker.Clear();

        var result = _itemDao.GetItemsByName(weapon.Name)
            .OfType<Weapon>()
            .Single();
        Assert.IsTrue(result.IsEquipped());
    }
    
    [TestMethod]
    public void EquippedArmor_RemainsEquippedAfterDatabaseReload()
    {
        var armor = CreateItem("Helmet", 5, 10, TestItemType.Armor) as Armor;
        armor!.Equip();
        _itemDao.UpdateItem(armor);
        _context.ChangeTracker.Clear();

        var result = _itemDao.GetItemsByName(armor.Name)
            .OfType<Armor>()
            .Single();
        Assert.IsTrue(result.IsEquipped());
    }

    [TestMethod]
    public void UnequippedWeapon_RemainsUnequippedAfterDatabaseReload()
    {
        var weapon = CreateItem("Sword", 1, 6, TestItemType.Weapon) as Weapon;
        weapon!.Equip();
        _itemDao.UpdateItem(weapon);
        _context.ChangeTracker.Clear();
        
        var weaponToUnequip = _itemDao.GetItemsByName(weapon.Name)
            .OfType<Weapon>()
            .Single();

        weaponToUnequip.Unequip();
        _itemDao.UpdateItem(weaponToUnequip);
        _context.ChangeTracker.Clear();
        
        var result = _itemDao.GetItemsByName(weapon.Name)
            .OfType<Weapon>()
            .Single();
        Assert.IsFalse(result.IsEquipped());
    }
    
    [TestMethod]
    public void UnequippedArmor_RemainsUnequippedAfterDatabaseReload()
    {
        var armor = CreateItem("Vambrace", 1, 5, TestItemType.Armor, ArmorType.Arms) as Armor;
        armor!.Equip();
        _itemDao.UpdateItem(armor);
        _context.ChangeTracker.Clear();
        
        var armorToUnequip = _itemDao.GetItemsByName(armor.Name)
            .OfType<Armor>()
            .Single();

        armorToUnequip.Unequip();
        _itemDao.UpdateItem(armorToUnequip);
        _context.ChangeTracker.Clear();

        var result = _itemDao.GetItemsByName(armor.Name)
            .OfType<Armor>()
            .Single();
        Assert.IsFalse(result.IsEquipped());
    }

    [TestMethod]
    public void NewWeapon_DefaultsToUnequippedAfterDatabaseReload()
    {
        var weapon = CreateItem("Lance", 1,  2, TestItemType.Weapon);
        _itemDao.AddItem(weapon);
        _context.ChangeTracker.Clear();

        var result = _itemDao.GetItemsByName(weapon.Name)
            .OfType<Weapon>()
            .Single();
        Assert.IsFalse(result.IsEquipped());
    }
    
    [TestMethod]
    public void NewArmor_DefaultsToUnequippedAfterDatabaseReload()
    {
        var armor = CreateItem("Shin Guards", 1, 1, TestItemType.Armor,  ArmorType.Legs);
        _itemDao.AddItem(armor);
        _context.ChangeTracker.Clear();

        var result = _itemDao.GetItemsByName(armor.Name)
            .OfType<Armor>()
            .Single();
        Assert.IsFalse(result.IsEquipped());
    }
}